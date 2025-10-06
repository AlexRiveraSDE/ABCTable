using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gestor centralizado de productos con soporte para análisis ABC.
/// Implementa el patrón Repository para abstraer el acceso a datos y centraliza
/// todas las operaciones CRUD sobre productos. Se relaciona con Product (datos),
/// LoadTable (persistencia) y ProgramConsole (interfaz de usuario).
/// </summary>
public class ABCManager
{
    // Almacén en memoria de productos (base de datos en memoria del sistema)
    private List<Product> products = new List<Product>();

    // Servicio de persistencia para guardar/cargar datos en JSON
    private readonly LoadTable _LoadService;

    /// <summary>
    /// Inicializa el gestor cargando productos desde JSON y aplicando clasificación ABC inicial.
    /// Se conecta con LoadTable para la persistencia y ejecuta ClassifyProducts() si hay datos.
    /// </summary>
    public ABCManager()
    {
        _LoadService = new LoadTable();
        products = _LoadService.LoadProducts();

        if (products.Count > 0)
        {
            ClassifyProducts();
        }
    }

    /// <summary>
    /// Agrega un nuevo producto al inventario con validación de integridad.
    /// Garantiza unicidad del código y persiste automáticamente en JSON.
    /// Reclasifica ABC tras la operación exitosa.
    /// </summary>
    public void AddProduct(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product), "El producto no puede ser null");
        
        if (products.Any(p => p.Code == product.Code))
            throw new InvalidOperationException($"Ya existe un producto con el código '{product.Code}'");
        
        products.Add(product);
        
        if (!ValidateAllProducts())
        {
            products.Remove(product);
            throw new InvalidOperationException("No se puede agregar el producto: datos inválidos detectados");
        }
        
        _LoadService.SaveProducts(products);
        ClassifyProducts();
        
        Console.WriteLine($"Producto '{product.Code}' agregado exitosamente");
    }

    /// <summary>
    /// Busca un producto por código (case-insensitive).
    /// Retorna null si no se encuentra, permitiendo manejo flexible de resultados.
    /// </summary>
    public Product? GetProduct(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        return products.FirstOrDefault(p => p.Code == code.ToUpper());
    }

    /// <summary>
    /// Obtiene una copia de todos los productos para evitar modificaciones externas.
    /// Mantiene la encapsulación de la lista interna.
    /// </summary>
    public List<Product> GetAll()
    {
        return products.ToList();
    }

    /// <summary>
    /// Actualiza un producto existente con validación de integridad y rollback automático.
    /// Reclasifica ABC solo si cambian campos relevantes (precio o movimientos).
    /// Persiste automáticamente en JSON tras la operación exitosa.
    /// </summary>
    public void UpdateProduct(string code, Product updatedProduct)
    {
        if (updatedProduct == null)
            throw new ArgumentNullException(nameof(updatedProduct));
        
        var existingProduct = GetProduct(code);
        if (existingProduct == null)
            throw new InvalidOperationException($"No existe un producto con el código '{code.ToUpper()}'");
        
        string originalName = existingProduct.Name;
        int originalMoves = existingProduct.MovesPerMonth;
        decimal originalPrice = existingProduct.UnitaryPrice;
        
        existingProduct.Name = updatedProduct.Name;
        existingProduct.MovesPerMonth = updatedProduct.MovesPerMonth;
        existingProduct.UnitaryPrice = updatedProduct.UnitaryPrice;
        
        if (!ValidateAllProducts())
        {
            existingProduct.Name = originalName;
            existingProduct.MovesPerMonth = originalMoves;
            existingProduct.UnitaryPrice = originalPrice;
            throw new InvalidOperationException("No se puede actualizar el producto: datos inválidos detectados");
        }
        
        _LoadService.SaveProducts(products);
        Console.WriteLine($"Producto '{existingProduct.Code}' actualizado exitosamente");
        
        bool movesChanged = originalMoves != updatedProduct.MovesPerMonth;
        bool priceChanged = originalPrice != updatedProduct.UnitaryPrice;
        
        if (movesChanged || priceChanged)
        {
            ClassifyProducts();
            Console.WriteLine("  → Tabla reclasificada automáticamente");
        }
    }

    /// <summary>
    /// Elimina un producto del inventario con validación de integridad.
    /// Retorna false si el producto no existe, permitiendo operaciones idempotentes.
    /// Reclasifica ABC tras la operación exitosa.
    /// </summary>
    public bool RemoveProduct(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            Console.WriteLine("El código no puede estar vacío");
            return false;
        }
        
        var product = GetProduct(code);
        if (product == null)
        {
            Console.WriteLine($"No se encontró producto con código '{code.ToUpper()}'");
            return false;
        }
        
        products.Remove(product);
        
        if (!ValidateAllProducts())
        {
            products.Add(product);
            Console.WriteLine("Error de integridad al eliminar producto. Operación cancelada.");
            return false;
        }
        
        _LoadService.SaveProducts(products);
        ClassifyProducts();
        
        Console.WriteLine($"Producto '{product.Code}' eliminado exitosamente");
        return true;
    }

    /// <summary>
    /// Ejecuta el algoritmo de clasificación ABC basado en la regla de Pareto (80/20).
    /// Asigna categorías A (≤80% acumulado), B (≤95% acumulado) y C (>95% acumulado).
    /// Ordena productos por TotalValue descendente y calcula porcentajes acumulados.
    /// </summary>
    public void ClassifyProducts()
    {
        if (products.Count == 0)
        {
            Console.WriteLine("No hay productos para clasificar");
            return;
        }

        decimal totalInventoryValue = products.Sum(p => p.TotalValue);

        if (totalInventoryValue == 0)
        {
            Console.WriteLine("El valor total del inventario es 0");
            foreach (var product in products)
            {
                product.Classification = "C";
                product.AccumulatedPercentage = 0;
            }
            return;
        }

        var sortedProducts = products.OrderByDescending(p => p.TotalValue).ToList();
        decimal accumulatedPercentage = 0;

        foreach (var product in sortedProducts)
        {
            decimal productPercentage = (product.TotalValue / totalInventoryValue) * 100;
            accumulatedPercentage += productPercentage;
            product.AccumulatedPercentage = accumulatedPercentage;

            if (accumulatedPercentage <= 80)
            {
                product.Classification = "A";
            }
            else if (accumulatedPercentage <= 95)
            {
                product.Classification = "B";
            }
            else
            {
                product.Classification = "C";
            }
        }

        Console.WriteLine($"Clasificación ABC completada para {products.Count} productos");
        Console.WriteLine($"Valor total del inventario: ${totalInventoryValue:N2}");
    }

    /// <summary>
    /// Muestra la tabla de productos formateada en consola, ordenada por valor descendente.
    /// Utilizado por ProgramConsole para la visualización del estado del inventario.
    /// </summary>
    public void ShowTable()
    {
        if (products.Count == 0)
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════╗");
            Console.WriteLine("║  TABLA DE PRODUCTOS - ANÁLISIS ABC           ║");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║  No hay productos registrados              ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝\n");
            return;
        }

        var sortedProducts = products.OrderByDescending(p => p.TotalValue).ToList();

        Console.WriteLine("\n══════════════════════════════════════════════════════════════════════");
        Console.WriteLine("            TABLA DE PRODUCTOS - ANÁLISIS ABC                         ");
        Console.WriteLine($"            Total de productos: {products.Count,-35} ");
        Console.WriteLine("══════════════════════════════════════════════════════════════════════");

        foreach (var product in sortedProducts)
        {
            product.ShowInfo();
            Console.WriteLine("║────────────────────────────────────────────────────────────────────║");
        }

        Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝\n");
    }

    /// <summary>
    /// Valida la integridad de todos los productos en la lista.
    /// Utiliza ValidateAfterDeserialization() de cada Product para detectar datos corruptos.
    /// Esencial para garantizar que nunca se persistan datos inválidos.
    /// </summary>
    private bool ValidateAllProducts()
    {
        foreach (var product in products)
        {
            try
            {
                product.ValidateAfterDeserialization();
            }
            catch (InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error de integridad de datos: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }
        return true;
    }
}