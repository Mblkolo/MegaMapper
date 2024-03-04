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
//Каждый метод маппинга получает нужный объект и загружает всё недостающее
Console.WriteLine();
Console.WriteLine(nameof(InnocentMapper));
var innocentMapper = new InnocentMapper();
var result2 = await innocentMapper.Execute(new[] { 1, 5, 8, 34 });
//Print(result2);


//Наивный маппер версия 2
//Каждый метод маппинга ид и грузит только один объект
Console.WriteLine();
Console.WriteLine(nameof(InnocentMapperV2));
var innocentMapperV2 = new InnocentMapperV2();
var result3 = await innocentMapper.Execute(new[] { 1, 5, 8, 34 });
Print(result3);


//GraphQLike маппер версия 1
//Все объекты одного порядка грузятся сразу одновременно
Console.WriteLine();
Console.WriteLine(nameof(GraphQLikeMapping));
var graphQLikeMapping = new GraphQLikeMapping();
var result4 = await graphQLikeMapping.Execute(new[] { 1, 5, 8, 34 });
Print(result4);

static void Print<T>(T @object)
{
    Console.WriteLine(JsonSerializer.Serialize(@object, new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new DateOnlyJsonConverter() },
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    }));
}