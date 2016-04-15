using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace MusicStore.Models
{
    public static class Dal
    {
        public static Task<List<string>> GetCartAlbumTitles(DbConnection connection, string shoppingCartId)
        {
            return Query(
                connection,
                @"SELECT [cart.Album].[Title]
                  FROM [CartItems] AS [cart]
                  INNER JOIN [Albums] AS [cart.Album] ON [cart].[AlbumId] = [cart.Album].[AlbumId]
                  WHERE [cart].[CartId] = @___shoppingCartId_0
                  ORDER BY [cart.Album].[Title]",
                1,
                values => (string) values[0],
                new Dictionary<string, object> {{"___shoppingCartId_0", shoppingCartId}});
        }

        public static Task<List<string>> GetGenreNames(DbConnection connection)
        {
            return Query(
                connection,
                @"SELECT TOP(@__p_0) [g].[Name]
                  FROM [Genres] AS [g]",
                1,
                values => (string) values[0],
                new Dictionary<string, object> {{"__p_0", 9}});
        }

        public static Task<List<Album>> GetTopSellingAlbumsAsync(DbConnection connection)
        {
            return Query(
                connection,
                @"SELECT TOP(@__p_0) [a].[AlbumId], [a].[AlbumArtUrl], [a].[ArtistId], [a].[Created], [a].[GenreId], [a].[Price], [a].[Title]
                  FROM [Albums] AS [a]
                  ORDER BY (
                      SELECT COUNT(*)
                      FROM [OrderDetails] AS [o]
                      WHERE [a].[AlbumId] = [o].[AlbumId]
                  ) DESC",
                7,
                values => new Album
                {
                    AlbumId = (int) values[0],
                    AlbumArtUrl = (string) values[1],
                    ArtistId = (int) values[2],
                    Created = (DateTime) values[3],
                    GenreId = (int) values[4],
                    Price = (decimal) values[5],
                    Title = (string) values[6]
                },
                new Dictionary<string, object> {{"__p_0", 6}});
        }

        private static async Task<List<T>> Query<T>(
            DbConnection connection,
            string sql,
            int rowSize,
            Func<object[], T> materializer,
            IDictionary<string, object> parameters)
        {
            var results = new List<T>();

            var openedConnection = false;

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();

                openedConnection = true;
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;

                foreach (var parameterValue in parameters)
                {
                    var parameter = command.CreateParameter();

                    parameter.ParameterName = parameterValue.Key;
                    parameter.Value = parameterValue.Value;

                    command.Parameters.Add(parameter);
                }

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            var values = new object[rowSize];
                            reader.GetValues(values);

                            results.Add(materializer(values));
                        }
                    }
                }
                finally
                {
                    if (openedConnection)
                    {
                        connection.Close();
                    }
                }
            }

            return results;
        }
    }
}