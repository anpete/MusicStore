using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MusicStore.ViewModels;

namespace MusicStore.Models
{
    public static class Dal
    {
        public static async Task<CartItem> GetCartItemForRemove(
            DbConnection connection, string shoppingCartId, int cartItemId)
        {
            var results = await Query(
                connection,
                @"SELECT TOP(1) [ci].[Count], [ci].[AlbumId], [ci.Album].[Title]
                  FROM [CartItems] AS [ci]
                  INNER JOIN [Albums] AS [ci.Album] ON [ci].[AlbumId] = [ci.Album].[AlbumId]
                  WHERE ([ci].[CartId] = @___shoppingCartId_0) AND ([ci].[CartItemId] = @__id_1)",
                3,
                values => new CartItem
                {
                    Count = (int) values[0],
                    Album = new Album
                    {
                        AlbumId = (int) values[1],
                        Title = (string) values[2]
                    }
                },
                new Dictionary<string, object>
                {
                    {"___shoppingCartId_0", shoppingCartId},
                    {"__id_1", cartItemId}
                });

            return results[0];
        }
        
        public static Task<List<CartItem>> GetCartItems(DbConnection connection, string shoppingCartId)
        {
            return Query(
                connection,
                @"SELECT [cart].[CartItemId], [cart].[AlbumId], [cart].[CartId], [cart].[Count], [cart].[DateCreated], 
                         [a].[AlbumId], [a].[AlbumArtUrl], [a].[ArtistId], [a].[Created], [a].[GenreId], [a].[Price], [a].[Title]
                  FROM [CartItems] AS [cart]
                  INNER JOIN [Albums] AS [a] ON [cart].[AlbumId] = [a].[AlbumId]
                  WHERE [cart].[CartId] = @___shoppingCartId_0",
                12,
                values => new CartItem
                {
                    CartItemId = (int) values[0],
                    AlbumId = (int) values[1],
                    CartId = (string) values[2],
                    Count = (int) values[3],
                    DateCreated = (DateTime) values[4],
                    Album = new Album
                    {
                        AlbumId = (int) values[5],
                        AlbumArtUrl = (string) values[6],
                        ArtistId = (int) values[7],
                        Created = (DateTime) values[8],
                        GenreId = (int) values[9],
                        Price = (decimal) values[10],
                        Title = (string) values[11]
                    }
                },
                new Dictionary<string, object> {{"___shoppingCartId_0", shoppingCartId}});
        }
        
        public static async Task<decimal> GetShoppingCartTotal(DbConnection connection, string shoppingCartId)
        {
            var results = await Query(
                connection,
                @"SELECT SUM([c.Album].[Price] * [c].[Count])
                  FROM [CartItems] AS [c]
                  INNER JOIN [Albums] AS [c.Album] ON [c].[AlbumId] = [c.Album].[AlbumId]
                  WHERE [c].[CartId] = @___shoppingCartId_0",
                1,
                values => ReferenceEquals(values[0], DBNull.Value) ? 0 : (decimal) values[0],
                new Dictionary<string, object>
                {
                    {"___shoppingCartId_0", shoppingCartId}
                });

            return results[0];
        }
        
        public static async Task<int> GetShoppingCartCount(DbConnection connection, string shoppingCartId)
        {
            var results = await Query(
                connection,
                @"SELECT SUM([c].[Count])
                  FROM [CartItems] AS [c]
                  WHERE ([c].[CartId] = @___shoppingCartId_0)",
                1,
                values => ReferenceEquals(values[0], DBNull.Value) ? 0 : (int) values[0],
                new Dictionary<string, object>
                {
                    {"___shoppingCartId_0", shoppingCartId}
                });

            return results[0];
        }
        
        public static async Task<CartItem> GetCartItem(DbConnection connection, string shoppingCartId, int albumId)
        {
            var results = await Query(
                connection,
                @"SELECT TOP(2) [c].[CartItemId], [c].[AlbumId], [c].[CartId], [c].[Count], [c].[DateCreated]
                  FROM [CartItems] AS [c]
                  WHERE ([c].[CartId] = @___shoppingCartId_0) AND ([c].[AlbumId] = @__albumId_1)",
                5,
                values => new CartItem
                {
                    CartItemId = (int) values[0],
                    AlbumId = (int) values[1],
                    CartId = (string) values[2],
                    Count = (int) values[3],
                    DateCreated = (DateTime) values[4]
                },
                new Dictionary<string, object>
                {
                    {"___shoppingCartId_0", shoppingCartId},
                    {"__albumId_1", albumId}
                });

            switch (results.Count)
            {
                case 0:
                    return null;
                case 1:
                    return results[0];
                default:
                    throw new InvalidOperationException();
            }
        }
        
        public static async Task<AlbumDetails> GetAlbumDetails(DbConnection connection, int albumId)
        {
            var results = await Query(
                connection,
                @"SELECT TOP(1) [a].[AlbumId], [a].[Title], [a].[AlbumArtUrl], [a].[Price], [a.Genre].[Name], [a.Artist].[Name]
                  FROM [Albums] AS [a]
                  INNER JOIN [Artists] AS [a.Artist] ON [a].[ArtistId] = [a.Artist].[ArtistId]
                  INNER JOIN [Genres] AS [a.Genre] ON [a].[GenreId] = [a.Genre].[GenreId]
                  WHERE [a].[AlbumId] = @__id_0",
                6,
                values => new AlbumDetails
                {
                    AlbumId = (int) values[0],
                    Title = (string) values[1],
                    AlbumArtUrl = (string) values[2],
                    Price = (decimal) values[3],
                    GenreName = (string) values[4],
                    ArtistName = (string) values[5]
                },
                new Dictionary<string, object> {{"__id_0", albumId}});

            return results.Count == 1 ? results[0] : null;
        }

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