// See https://aka.ms/new-console-template for more information

using System.Text.Encodings.Web;
using System.Text.Json;
using MegaMapper.Json;
using MegaMapper.Mappers;


//Классический маппер.
//Сначала всё грузим, потом мапим чистыми функциями.
Console.WriteLine(nameof(ClassicMappings));
var classicMappings = new ClassicMappings();
var result = await classicMappings.Execute(new[]{1, 5, 8, 34});
//Print(result);

//Наивный маппер
//Каждый метод мапинга получает нужный объект и загружает всё недостающее
Console.WriteLine();
Console.WriteLine(nameof(InnocentMapper));
var innocentMapper = new InnocentMapper();
var result2 = await innocentMapper.Execute(new[] { 1, 5, 8, 34 });
Print(result2);




static void Print<T>(T @object)
{
    Console.WriteLine(JsonSerializer.Serialize(@object, new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new DateOnlyJsonConverter() },
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    }));
}