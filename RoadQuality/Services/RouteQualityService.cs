using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoadQuality.Configurations;
using RoadQuality.Models;
using RoadQuality.DTO;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Entities;
using MongoDB.Bson;
using System.Net.Http;
using System.Text.Json;
using System.Net;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization;

namespace RoadQuality.Services
{
    public class RouteQualityService
    {
        private readonly IMongoCollection<QualityPoint> _points;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IDatabaseSettings _settings;
        public RouteQualityService(IDatabaseSettings settings, IHttpClientFactory clientFactory)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _clientFactory = clientFactory;
            _points = database.GetCollection<QualityPoint>(settings.RoadQualityCollectionName);
            _settings = settings;

            _points.Indexes.CreateOneAsync(new CreateIndexModel<QualityPoint>(Builders<QualityPoint>.IndexKeys.Geo2DSphere(it => it.Location)));
        }

        public async Task<List<QualityPoint>> GetAllRaw()
        {
            return (await _points.FindAsync(point => true)).ToList();
        }

        public async Task<List<QualityPointDTO>> GetAll()
        {
            FindOptions<QualityPoint, QualityPointDTO> projection = QualityPointDTOProjection();
            FilterDefinition<QualityPoint> filter = new ExpressionFilterDefinition<QualityPoint>(_ => true);

            return (await _points.FindAsync(filter, projection)).ToList();
        }

        public async Task<List<QualityPointDTO>> GetPoints(FilterQuery query, string user = null)
        {
            if (!query.End.HasValue && !query.Start.HasValue && ((query.OnlyLoggedUserData ?? false) == false))
            {
                return await GetAll();
            }

            var qualityPointsUnwind = _points.Aggregate()
                .Unwind<QualityPoint, QualityPointUnwind>(p => p.Data);

            qualityPointsUnwind = MatchPointsByDateAndUser(qualityPointsUnwind, query.Start, query.End, (query.OnlyLoggedUserData ?? false) ? user : null);

            var qualityPointList = await GroupAndFetchUnwind(qualityPointsUnwind);

            return qualityPointList;
        }

        public async Task<List<QualityPointDTO>> GetPointsByGeo(GeoQueryDTO query, string user = null)
        {
            user ??= "";
            var geoPoint = GeoJson.Point(GeoJson.Geographic(query.Longitude, query.Latitude));
            var radius = query.Radius.HasValue ? query.Radius : 2500;

            var locationQuery = Builders<QualityPoint>.Filter.Near(tag => tag.Location, geoPoint, radius);
            FindOptions<QualityPoint, QualityPointDTO> projection = QualityPointDTOProjection();

            var list = await _points.Find(locationQuery).ToListAsync();
            var qualityPointDTOList = new List<QualityPointDTO>();

            var wasFiltered = false;
            if (query.Start.HasValue)
            {
                list.ForEach(point =>
                {
                    point.Data = point.Data.Where(data => data.Date >= query.Start && data.Date <= query.End).ToList();
                });
                wasFiltered = true;
            }
            if (query.OnlyLoggedUserData.HasValue && query.OnlyLoggedUserData == true && user.Length > 0)
            {
                list.ForEach(point =>
                {
                    point.Data = point.Data.Where(data => data.UserId == user).ToList();
                });
                wasFiltered = true;
            }

            list.ForEach(p =>
            {
                if (wasFiltered)
                {
                    if (p.Data.Count == 0)
                    {
                        return;
                    }
                    qualityPointDTOList.Add(new()
                    {
                        Vector = GetAvgVector(p.Data),
                        Speed = GetAvgSpeed(p.Data),
                        Location = new GeoPoint { Latitude = p.Location.Coordinates.Latitude, Longitude = p.Location.Coordinates.Longitude }
                    });
                } else
                {
                    qualityPointDTOList.Add(new()
                    {
                        Vector = p.AvgVector,
                        Speed = p.AvgSpeed,
                        Location = new GeoPoint { Latitude = p.Location.Coordinates.Latitude, Longitude = p.Location.Coordinates.Longitude }
                    });
                }                
            });
            return qualityPointDTOList;
        }

        public async Task AddPoint(QualityPoint point, string userId)
        {
            QualityPoint nearPoint = await GetNearPoint(point);
            if (nearPoint == null)
            {
                await _points.InsertOneAsync(point);
                return;
            } else
            {
                PointDataObject pointDetails = new()
                {
                    Speed = point.AvgSpeed,
                    Vector = point.AvgVector,
                    Date = DateTime.Now,
                    UserId = userId
                };
                nearPoint.Data.Add(pointDetails);
                nearPoint.AvgVector = GetAvgVector(nearPoint.Data);
                nearPoint.AvgSpeed = GetAvgSpeed(nearPoint.Data);

                var filter = Builders<QualityPoint>.Filter.Eq(x => x.Id, nearPoint.Id);
                await  _points.ReplaceOneAsync(filter, nearPoint);
                return;
            }
        }

        public async Task AddPoint(RoadPointDTO point, string userId)
        {
            QualityPoint qPoint = new QualityPoint {
                AvgVector = point.Vector,
                AvgSpeed = point.Speed,
                Data = new List<PointDataObject>
                {
                    new PointDataObject()
                    {
                        Speed = point.Speed,
                        Vector = point.Vector,
                        UserId = userId,
                        Date = DateTime.Now,
                    }
                },
                Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                                new GeoJson2DGeographicCoordinates(point.Longitude, point.Latitude))
            };
            GeoPoint snappedPoint = await SnapToRoad(qPoint);
            if(snappedPoint != null)
            {
                qPoint.Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                                new GeoJson2DGeographicCoordinates(snappedPoint.Longitude, snappedPoint.Latitude));
            }
            await AddPoint(qPoint, userId);
        }

        private async Task<QualityPoint> GetNearPoint(QualityPoint point)
        {
            var geoPoint = GeoJson.Point(GeoJson.Geographic(point.Location.Coordinates.Longitude, point.Location.Coordinates.Latitude));
            var locationQuery = new FilterDefinitionBuilder<QualityPoint>().Near(tag => tag.Location, geoPoint, 5);
            var query = _points.Find(locationQuery).Limit(1);
            return await query.FirstOrDefaultAsync();
        }

        public async Task RemoveAll()
        {
            await _points.DeleteManyAsync(_ => true);
            return;
        }

        private async Task<GeoPoint> SnapToRoad(QualityPoint point)
        {
            var lng = point.Location.Coordinates.Longitude.ToString().Replace(",", ".");
            var lat = point.Location.Coordinates.Latitude.ToString().Replace(",", ".");

            try
            {
                string json = (new WebClient()).DownloadString(_settings.OsrmAdrress + "nearest/v1/driving/" + lng + "," + lat);
                var responseContent = JsonConvert.DeserializeObject<OsrmResponse>(json);

                if (responseContent == null)
                {
                    return null;
                }

                var location = responseContent.Waypoints.First().Location;

                GeoPoint result = new()
                {
                    Longitude = Double.Parse(location.First().Replace(".", ",")),
                    Latitude = Double.Parse(location.Last().Replace(".", ",")),
                };

                return result;
            } catch
            {
                return null;
            }            
        }

        private FindOptions<QualityPoint, QualityPointDTO> QualityPointDTOProjection()
        {
            return new FindOptions<QualityPoint, QualityPointDTO>
            {
                Projection = Builders<QualityPoint>.Projection.Expression(p => new QualityPointDTO
                {
                    Vector = p.AvgVector,
                    Speed = p.AvgSpeed,
                    Location = new GeoPoint { Latitude = p.Location.Coordinates.Latitude, Longitude = p.Location.Coordinates.Longitude }
                })
            };
        }

        private double GetAvgVector(List<PointDataObject> list)
        {
            return list.Count > 0 ? list.Select(item => item.Vector).Aggregate((seq, acc) => seq + acc) / list.Count : 0.0;
        }

        private double GetAvgSpeed(List<PointDataObject> list)
        {
            return list.Count > 0 ? list.Select(item => item.Speed).Aggregate((seq, acc) => seq + acc) / list.Count : 0.0;
        }

        private IAggregateFluent<QualityPointUnwind> MatchPointsByDateAndUser(IAggregateFluent<QualityPointUnwind> point, DateTime? start, DateTime? end, string userId)
        {
            if (start.HasValue && end.HasValue && userId?.Length > 0)
            {
                point = point.Match(p => p.Data.Date >= start && p.Data.Date <= end && p.Data.UserId == userId);
            }
            else if (start.HasValue && end.HasValue)
            {
                point = point.Match(p => p.Data.Date >= start && p.Data.Date <= end);
            }
            else if (userId?.Length > 0)
            {
                point = point.Match(p => p.Data.UserId == userId);
            }
            return point;
        }

        private async Task<List<QualityPointDTO>> GroupAndFetchUnwind(IAggregateFluent<QualityPointUnwind> point)
        {
            var qualityPointList = await point
                .Group(
                    x => x.Id,
                    g => new QualityPoint
                    {
                        Id = g.Key,
                        Location = g.First().Location,
                        AvgSpeed = g.Select(x => x.Data.Speed).Average(),
                        AvgVector = g.Select(x => x.Data.Vector).Average()
                    }
                ).ToListAsync();

            List<QualityPointDTO> qualityPointDTOList = new();

            qualityPointList.ForEach(el =>
            {
                qualityPointDTOList.Add(new QualityPointDTO()
                {
                    Speed = el.AvgSpeed,
                    Vector = el.AvgVector,
                    Location = new GeoPoint { Latitude = el.Location.Coordinates.Latitude, Longitude = el.Location.Coordinates.Longitude }
                });
            });

            return qualityPointDTOList;
        }
    }
}
