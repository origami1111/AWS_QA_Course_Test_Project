using AWS_QA_Course_Test_Project.DTOs;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AWS_QA_Course_Test_Project.Utils
{
    public class DBUtilsHandler
    {
        public static async Task<ImageDBEntity> GetImageAsync(MySqlConnection connection, string id)
        {
            var images = await GetImagesAsync(connection);
            return images.Find(image => image.Id == int.Parse(id));
        }

        public static async Task<List<ImageDBEntity>> GetImagesAsync(MySqlConnection connection)
        {
            var images = new List<ImageDBEntity>();
            string query = "SELECT * FROM cloudximages.images";

            using (var command = new MySqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var image = new ImageDBEntity
                        {
                            Id = reader.GetInt32("id"),
                            ObjectKey = reader.GetString("object_key"),
                            ObjectSize = reader.GetInt32("object_size"),
                            ObjectType = reader.GetString("object_type"),
                            LastModified = reader.GetDateTime("last_modified").ToString("yyyy-MM-ddTHH:mm:ssZ")
                        };
                        images.Add(image);
                    }
                }
            }

            return images;
        }
    }
}
