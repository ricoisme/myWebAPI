using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace myWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class MyTestController : ControllerBase
    {
        private readonly ICityNameService _cityNameService;
        public MyTestController(ICityNameService cityNameService)
        {
            _cityNameService = cityNameService;
        }
        [HttpPut("myupdate")]
        public async Task<ApiResponse> Update(UpdateRequest updateRequest)
        {
            await _cityNameService.UpdateAsync(new CityName { Id = updateRequest.Id, Name = updateRequest.Name });
            var res = new ApiResponse()
            {
                StatusCode = "200",
                Error = ""
            };
            return await Task.FromResult(res);
        }

        [HttpDelete("mydelete")]
        public async Task<ApiResponse> Delete(int id)
        {
            await _cityNameService.DeleteAsync(id);
            var res = new ApiResponse()
            {
                StatusCode = "200",
                Error = ""
            };
            return await Task.FromResult(res);
        }
        [HttpGet("{name}")]
        public async Task<ApiResponse<IEnumerable<CityName>>> Select(string name)
        {
            var result = await _cityNameService.SelectAsync(name);
            var res = new ApiResponse<IEnumerable<CityName>>
            {
                Error = "",
                StatusCode = "200",
                Data = result
            };
            return await Task.FromResult(res);
        }

        public async Task<ApiResponse> Create(UpdateRequest request)
        {
            await _cityNameService.AddAsync(new CityName { Id = request.Id, Name = request.Name });
            var res = new ApiResponse()
            {
                Error = "",
                StatusCode = "200"
            };
            return await Task.FromResult(res);
        }
    }


    public interface ICityNameService
    {
        Task AddAsync(CityName cityName);
        Task DeleteAsync(int id);
        Task UpdateAsync(CityName cityName);
        Task<IEnumerable<CityName>> SelectAsync(string name);
    }

    public class CityNameService : ICityNameService
    {
        private readonly string _connectionString;
        public CityNameService(string connectionString)
        {
            _connectionString = connectionString;
        }
        async Task ICityNameService.AddAsync(CityName cityName)
        {
            string sql = "insert into dbo.cityname values(@id,@name)";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add(
                            new SqlParameter
                            { DbType = System.Data.DbType.Int32, ParameterName = "@id", Value = cityName.Id });
                        cmd.Parameters.Add(
                            new SqlParameter { DbType = System.Data.DbType.AnsiString, ParameterName = "@name", Value = cityName.Name }
                            );
                        if (conn.State == System.Data.ConnectionState.Closed)
                        {
                            await conn.OpenAsync();
                        }
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        async Task ICityNameService.DeleteAsync(int id)
        {
            string sql = "delete dbo.cityname where id=@id";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(
                        new SqlParameter
                        {
                            ParameterName = "@id",
                            Value = id,
                            DbType = System.Data.DbType.Int32
                        }
                        );
                    if (conn.State == System.Data.ConnectionState.Closed)
                    {
                        await conn.OpenAsync();
                    }
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        async Task<IEnumerable<CityName>> ICityNameService.SelectAsync(string name)
        {
            var result = new List<CityName>();
            string sql = "select id,CName from dbo.cityname where CName=@name";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = "@name",
                        Value = name,
                        DbType = System.Data.DbType.AnsiString,
                        Size = 30
                    });
                    if (conn.State == System.Data.ConnectionState.Closed)
                    {
                        await conn.OpenAsync();
                    }
                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        while (dr.Read())
                        {
                            result.Add
                                (
                                new CityName
                                {
                                    Id = (int)dr["Id"],
                                    Name = dr["CName"].ToString()
                                }
                                );
                            //yield return new CityName { Id = (int)dr["Id"], Name = dr["CName"].ToString() };
                        }
                    }
                    return result;
                }
            }
        }

        async Task ICityNameService.UpdateAsync(CityName cityName)
        {
            string sql = "update dbo.cityname set CName=@name where id=@id";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = "@id",
                        Value = cityName.Id,
                        DbType = System.Data.DbType.Int32
                    });
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = "@name",
                        Value = cityName.Name,
                        DbType = System.Data.DbType.AnsiString,
                        Size = 30
                    });
                    if (conn.State == System.Data.ConnectionState.Closed)
                    {
                        await conn.OpenAsync();
                    }
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }

    public class CityName
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ApiResponse
    {
        public string StatusCode { get; set; }
        public string Error { get; set; }
    }

    public class ApiResponse<T>
    {
        public string StatusCode { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }
    }

    public class UpdateRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}