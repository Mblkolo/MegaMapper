// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using MegaMapper.Json;
using MegaMapper.Mappers;


var classicMappings = new ClassicMappings();
var result = await classicMappings.Excecute(new[]{1, 5, 8, 34});
Print(result);



static void Print<T>(T @object)
{
    Console.WriteLine(JsonSerializer.Serialize(@object, new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new DateOnlyJsonConverter() },
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    }));
}