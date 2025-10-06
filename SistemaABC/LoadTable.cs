using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

/// <summary>
/// Servicio especializado en persistencia de datos mediante archivos JSON.
/// Implementa el patrón Service Pattern para abstraer la lógica de lectura/escritura.
/// Se relaciona con Product (datos a persistir) y ABCManager (consumidor de la persistencia).
/// </summary>
public class LoadTable
{
    // Ruta del archivo JSON donde se almacenan los productos
    private readonly string _filePath;
    
    // Opciones de configuración para la serialización/deserialización JSON
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Inicializa el servicio de persistencia con la ruta de archivo especificada.
    /// Configura opciones JSON para formato legible y lectura flexible de propiedades.
    /// </summary>
    public LoadTable(string fileName = "exports/products.json")
    {
        _filePath = fileName;
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Guarda la lista de productos en archivo JSON con manejo de errores robusto.
    /// Crea automáticamente el directorio si no existe y proporciona retroalimentación visual.
    /// </summary>
    public bool SaveProducts(List<Product> products)
    {
        try
        {
            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Directorio '{directory}' creado");
                Console.ResetColor();
            }
            
            string jsonString = JsonSerializer.Serialize(products, _options);
            File.WriteAllText(_filePath, jsonString);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Datos guardados: {products.Count} producto(s) en '{_filePath}'");
            Console.ResetColor();
            
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Sin permisos para escribir en '{_filePath}'");
            Console.ResetColor();
            return false;
        }
        catch (IOException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error de E/S: {ex.Message}");
            Console.ResetColor();
            return false;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error al guardar: {ex.Message}");
            Console.ResetColor();
            return false;
        }
    }

    /// <summary>
    /// Carga productos desde archivo JSON con validación post-deserialización.
    /// Filtra productos inválidos y proporciona retroalimentación detallada al usuario.
    /// Maneja graciosamente casos especiales (archivo inexistente, vacío o corrupto).
    /// </summary>
    public List<Product> LoadProducts()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Archivo '{_filePath}' no encontrado. Iniciando con inventario vacío.");
                Console.ResetColor();
                return new List<Product>();
            }
            
            string jsonString = File.ReadAllText(_filePath);
            
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Archivo '{_filePath}' está vacío. Iniciando con inventario vacío.");
                Console.ResetColor();
                return new List<Product>();
            }
            
            var products = JsonSerializer.Deserialize<List<Product>>(jsonString, _options);
            
            if (products == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"No se pudieron cargar datos. Iniciando con inventario vacío.");
                Console.ResetColor();
                return new List<Product>();
            }
            
            var validProducts = new List<Product>();
            var invalidCount = 0;
            
            foreach (var product in products)
            {
                try
                {
                    product.ValidateAfterDeserialization();
                    validProducts.Add(product);
                }
                catch (InvalidOperationException ex)
                {
                    invalidCount++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"⚠ Producto inválido ignorado: {ex.Message}");
                    Console.ResetColor();
                }
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{validProducts.Count} producto(s) válido(s) cargado(s) desde '{_filePath}'");
            if (invalidCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠ {invalidCount} producto(s) inválido(s) ignorado(s)");
                Console.ResetColor();
            }
            Console.ResetColor();
            
            return validProducts;
        }
        catch (JsonException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: El archivo JSON es inválido");
            Console.WriteLine($"Detalle: {ex.Message}");
            Console.WriteLine($"Iniciando con inventario vacío.");
            Console.ResetColor();
            return new List<Product>();
        }
        catch (IOException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error de lectura: {ex.Message}");
            Console.WriteLine($"Iniciando con inventario vacío.");
            Console.ResetColor();
            return new List<Product>();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error inesperado al cargar: {ex.Message}");
            Console.WriteLine($"Iniciando con inventario vacío.");
            Console.ResetColor();
            return new List<Product>();
        }
    }

    /// <summary>
    /// Verifica si el archivo de datos existe en el sistema de archivos.
    /// </summary>
    public bool FileExists()
    {
        return File.Exists(_filePath);
    }

    /// <summary>
    /// Obtiene la ruta absoluta del archivo de datos para depuración o diagnóstico.
    /// </summary>
    public string GetFilePath()
    {
        return Path.GetFullPath(_filePath);
    }

    /// <summary>
    /// Elimina el archivo de datos del sistema (operación destructiva).
    /// Útil para reiniciar el inventario o pruebas de integración.
    /// </summary>
    public bool DeleteFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Archivo '{_filePath}' eliminado");
                Console.ResetColor();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error al eliminar archivo: {ex.Message}");
            Console.ResetColor();
            return false;
        }
    }
}