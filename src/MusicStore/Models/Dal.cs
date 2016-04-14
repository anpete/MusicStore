using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MusicStore.Models;
using System.Data.SqlClient;

namespace MusicStore.Models
{
    public static class Dal
    {
        public static async Task<List<Album>> GetTopSellingAlbumsAsync()
        {
            var albums = new List<Album>();
            
            using (var connection = new SqlConnection(
                @"Server=(localdb)\MSSQLLocalDB;Database=MusicStore;Trusted_Connection=True;MultipleActiveResultSets=true;Connect Timeout=30;"))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText
                        = @"SELECT TOP(@__p_0) [a].[AlbumId], [a].[AlbumArtUrl], [a].[ArtistId], [a].[Created], [a].[GenreId], [a].[Price], [a].[Title]
                            FROM [Albums] AS [a]
                            ORDER BY (
                                SELECT COUNT(*)
                                FROM [OrderDetails] AS [o]
                                WHERE [a].[AlbumId] = [o].[AlbumId]
                            ) DESC";

                    var parameter = command.CreateParameter();

                    parameter.ParameterName = "__p_0";
                    parameter.Value = 6;

                    command.Parameters.Add(parameter);

                    using (var reader = command.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            var values = new object[7];
                            reader.GetValues(values);
                            
                            albums.Add(
                                new Album
                                {
                                    AlbumId = (int) values[0],
                                    AlbumArtUrl = (string) values[1],
                                    ArtistId = (int) values[2],
                                    Created = (DateTime) values[3],
                                    GenreId = (int) values[4],
                                    Price = (decimal) values[5],
                                    Title = (string) values[6]
                                });
                        }
                    }
                }
            }
            
            return albums;
        }
    }
}