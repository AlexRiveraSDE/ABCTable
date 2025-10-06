using System;

/// <summary>
/// Punto de entrada principal del sistema WMS con interfaz de consola.
/// Coordina la interacción del usuario con ABCManager y proporciona un menú CRUD intuitivo.
/// Implementa el flujo de trabajo completo: carga inicial, menú interactivo y persistencia automática.
/// </summary>
class ProgramConsole
{
    // Instancia global del gestor de productos (Singleton implícito)
    // ABCManager se inicializa automáticamente y carga datos desde JSON al crear la instancia
    private static ABCManager manager = new ABCManager();

    /// <summary>
    /// Método principal que orquesta el flujo de la aplicación.
    /// Configura la consola, muestra mensajes de bienvenida/estado y ejecuta el bucle del menú.
    /// </summary>
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        ShowWelcome();
        ShowLoadStatus();
        
        bool exit = false;
        while (!exit)
        {
            ShowMenu();
            exit = ProcessOption();
        }
        
        ShowGoodbye();
    }

    /// <summary>
    /// Muestra el mensaje de bienvenida con formato visual atractivo.
    /// Establece el tono profesional del sistema WMS.
    /// </summary>
    static void ShowWelcome()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                                    ║");
        Console.WriteLine("║          SISTEMA DE ANÁLISIS ABC PARA WMS                          ║");
        Console.WriteLine("║          Gestión de Inventario por Clasificación                   ║");
        Console.WriteLine("║          Con Persistencia Automática en JSON                       ║");
        Console.WriteLine("║                                                                    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Muestra el estado inicial del inventario después de la carga automática.
    /// Proporciona retroalimentación inmediata sobre la cantidad de productos cargados.
    /// </summary>
    static void ShowLoadStatus()
    {
        int productCount = manager.GetAll().Count;
        
        if (productCount > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Inventario cargado: {productCount} producto(s)");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Inventario vacío - Puede cargar datos de ejemplo o agregar productos");
            Console.ResetColor();
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Muestra el menú principal de operaciones CRUD disponibles.
    /// Proporciona una interfaz clara y consistente para la interacción del usuario.
    /// </summary>
    static void ShowMenu()
    {
        Console.WriteLine("\n╔════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                         MENÚ PRINCIPAL                             ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  1. Ver tabla de productos                                         ║");
        Console.WriteLine("║  2. Agregar producto                                               ║");
        Console.WriteLine("║  3. Buscar producto                                                ║");
        Console.WriteLine("║  4. Modificar producto                                             ║");
        Console.WriteLine("║  5. Eliminar producto                                              ║");
        Console.WriteLine("║  0. Salir                                                          ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
        Console.Write("\nSeleccione una opción: ");
    }

    /// <summary>
    /// Muestra el mensaje de despedida al finalizar la aplicación.
    /// Proporciona una experiencia de usuario completa y profesional.
    /// </summary>
    static void ShowGoodbye()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nSaliendo...");
        Console.ResetColor();
    }

    /// <summary>
    /// Procesa la opción seleccionada por el usuario y delega a los métodos correspondientes.
    /// Maneja la lógica de navegación del menú y la validación de entradas.
    /// </summary>
    static bool ProcessOption()
    {
        string? option = Console.ReadLine();
        Console.WriteLine();
        
        switch (option)
        {
            case "1":
                ShowProducts();
                break;
            case "2":
                AddProduct();
                break;
            case "3":
                SearchProduct();
                break;
            case "4":
                UpdateProduct();
                break;
            case "5":
                RemoveProduct();
                break;
            case "0":
                return true;
            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Opción inválida. Por favor intente nuevamente.");
                Console.ResetColor();
                break;
        }
        
        Console.WriteLine("\nPresione ENTER para continuar...");
        Console.ReadLine();
        return false;
    }

    /// <summary>
    /// Muestra la tabla completa de productos utilizando el método ShowTable de ABCManager.
    /// Presenta los datos ordenados por valor descendente con formato tabular.
    /// </summary>
    static void ShowProducts()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("                    VER TABLA DE PRODUCTOS");
        Console.ResetColor();
        manager.ShowTable();
    }

    /// <summary>
    /// Permite al usuario agregar un nuevo producto con validación de entradas.
    /// Crea una instancia de Product y la delega a ABCManager para persistencia y clasificación.
    /// </summary>
    static void AddProduct()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("                      AGREGAR PRODUCTO");
        Console.ResetColor();
        
        try
        {
            Console.Write("Código del producto (ej: SKU001): ");
            string code = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(code))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nError: El código no puede estar vacío");
                Console.ResetColor();
                return;
            }
            
            Console.Write("Nombre del producto: ");
            string name = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nError: El nombre no puede estar vacío");
                Console.ResetColor();
                return;
            }
            
            Console.Write("Movimientos mensuales: ");
            string movesInput = Console.ReadLine() ?? "0";
            if (!int.TryParse(movesInput, out int moves) || moves < 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nError: Los movimientos deben ser un número mayor o igual a 0");
                Console.ResetColor();
                return;
            }
            
            Console.Write("Precio unitario: $");
            string priceInput = Console.ReadLine() ?? "0";
            if (!decimal.TryParse(priceInput, out decimal price) || price < 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n Error: El precio debe ser un número mayor o igual a 0");
                Console.ResetColor();
                return;
            }
            
            var product = new Product(code, name, moves, price);
            manager.AddProduct(product);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n Producto agregado exitosamente");
            Console.WriteLine("Datos guardados en exports/products.json");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {ex.Message}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Busca un producto por código y muestra su información detallada.
    /// Utiliza el método GetProduct de ABCManager para la búsqueda.
    /// </summary>
    static void SearchProduct()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("                      BUSCAR PRODUCTO");
        Console.ResetColor();
        
        Console.Write("Ingrese el código del producto: ");
        string code = Console.ReadLine() ?? "";
        
        var product = manager.GetProduct(code);
        
        if (product == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nNo se encontró un producto con el código '{code}'");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nProducto encontrado:");
            Console.ResetColor();
            Console.WriteLine();
            product.ShowInfo();
        }
    }

    /// <summary>
    /// Proporciona una interfaz interactiva para modificar productos existentes.
    /// Permite actualizar campos individuales o todos a la vez con confirmación explícita.
    /// Delega la actualización real a ABCManager para validación y persistencia.
    /// </summary>
    static void UpdateProduct()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("                    MODIFICAR PRODUCTO");
        Console.ResetColor();
        
        Console.Write("Ingrese el código del producto a modificar: ");
        string code = Console.ReadLine() ?? "";
        
        var existingProduct = manager.GetProduct(code);
        
        if (existingProduct == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nNo se encontró un producto con el código '{code}'");
            Console.ResetColor();
            return;
        }
        
        bool continueEditing = true;
        while (continueEditing)
        {
            Console.WriteLine("\n┌────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ DATOS ACTUALES DEL PRODUCTO                                        │");
            Console.WriteLine("└────────────────────────────────────────────────────────────────────┘");
            existingProduct.ShowInfo();
            
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              ¿QUÉ CAMPO DESEA MODIFICAR?                           ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║  1. Nombre                                                         ║");
            Console.WriteLine("║  2. Movimientos mensuales                                          ║");
            Console.WriteLine("║  3. Precio unitario                                                ║");
            Console.WriteLine("║  4. Modificar todos los campos                                     ║");
            Console.WriteLine("║  0. Terminar modificación                                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
            Console.Write("\nSeleccione una opción: ");
            
            string? option = Console.ReadLine();
            Console.WriteLine();
            
            try
            {
                bool changesMade = false;

                switch (option)
                {
                    case "1":
                        changesMade = UpdateName(existingProduct);
                        break;
                    case "2":
                        changesMade = UpdateMoves(existingProduct);
                        break;
                    case "3":
                        changesMade = UpdatePrice(existingProduct);
                        break;
                    case "4":
                        changesMade = UpdateAllFields(existingProduct);
                        break;
                    case "0":
                        continueEditing = false;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Modificación finalizada.");
                        Console.ResetColor();
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Opción inválida. Intente nuevamente.");
                        Console.ResetColor();
                        break;
                }

                if (changesMade)
                {
                    var updatedProduct = new Product(
                        existingProduct.Code, 
                        existingProduct.Name, 
                        existingProduct.MovesPerMonth, 
                        existingProduct.UnitaryPrice
                    );
                    manager.UpdateProduct(code, updatedProduct);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError: {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    /// <summary>
    /// Actualiza el nombre del producto con validación de entrada.
    /// Retorna true si se realizó el cambio, false si se canceló.
    /// </summary>
    static bool UpdateName(Product product)
    {
        Console.Write($"Nombre actual: {product.Name}\n");
        Console.Write("Nuevo nombre: ");
        string newName = Console.ReadLine() ?? "";
        
        if (string.IsNullOrWhiteSpace(newName))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("El nombre no puede estar vacío. Cambio cancelado.");
            Console.ResetColor();
            return false;
        }
        
        product.Name = newName;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Nombre actualizado exitosamente");
        Console.ResetColor();
        return true;
    }

    /// <summary>
    /// Actualiza los movimientos mensuales del producto con validación numérica.
    /// Retorna true si se realizó el cambio, false si se canceló.
    /// </summary>
    static bool UpdateMoves(Product product)
    {
        Console.Write($"Movimientos actuales: {product.MovesPerMonth}\n");
        Console.Write("Nuevos movimientos mensuales: ");
        string movesInput = Console.ReadLine() ?? "";
        
        if (!int.TryParse(movesInput, out int newMoves))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Formato inválido. Debe ser un número entero. Cambio cancelado.");
            Console.ResetColor();
            return false;
        }
        
        if (newMoves < 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Los movimientos no pueden ser negativos. Cambio cancelado.");
            Console.ResetColor();
            return false;
        }
        
        product.MovesPerMonth = newMoves;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Movimientos actualizados exitosamente");
        Console.ResetColor();
        return true;
    }

    /// <summary>
    /// Actualiza el precio unitario del producto con validación decimal.
    /// Retorna true si se realizó el cambio, false si se canceló.
    /// </summary>
    static bool UpdatePrice(Product product)
    {
        Console.Write($"Precio actual: ${product.UnitaryPrice:N2}\n");
        Console.Write("Nuevo precio unitario: $");
        string priceInput = Console.ReadLine() ?? "";
        
        if (!decimal.TryParse(priceInput, out decimal newPrice))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Formato inválido. Debe ser un número. Cambio cancelado.");
            Console.ResetColor();
            return false;
        }
        
        if (newPrice < 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("El precio no puede ser negativo. Cambio cancelado.");
            Console.ResetColor();
            return false;
        }
        
        product.UnitaryPrice = newPrice;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Precio actualizado exitosamente");
        Console.ResetColor();
        return true;
    }

    /// <summary>
    /// Actualiza todos los campos del producto permitiendo entradas parciales.
    /// Retorna true si se modificó al menos un campo, false si no hubo cambios.
    /// </summary>
    static bool UpdateAllFields(Product product)
    {
        Console.WriteLine("Ingrese los nuevos valores para todos los campos:\n");
        
        bool changed = false;

        Console.Write($"Nombre [{product.Name}]: ");
        string nameInput = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(nameInput))
        {
            product.Name = nameInput;
            changed = true;
        }
        
        Console.Write($"Movimientos mensuales [{product.MovesPerMonth}]: ");
        string movesInput = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(movesInput) && int.TryParse(movesInput, out int newMoves) && newMoves >= 0)
        {
            product.MovesPerMonth = newMoves;
            changed = true;
        }
        
        Console.Write($"Precio unitario [{product.UnitaryPrice:N2}]: $");
        string priceInput = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(priceInput) && decimal.TryParse(priceInput, out decimal newPrice) && newPrice >= 0)
        {
            product.UnitaryPrice = newPrice;
            changed = true;
        }
        
        if (changed)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n Campos actualizados exitosamente");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine("\nNo se realizaron cambios.");
        }

        return changed;
    }

    /// <summary>
    /// Elimina un producto del inventario con confirmación explícita del usuario.
    /// Muestra los datos del producto antes de la eliminación para evitar errores.
    /// </summary>
    static void RemoveProduct()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("                     ELIMINAR PRODUCTO");
        Console.ResetColor();

        Console.Write("Ingrese el código del producto a eliminar: ");
        string code = Console.ReadLine() ?? "";

        var product = manager.GetProduct(code);

        if (product == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nNo se encontró un producto con el código '{code}'");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("\nProducto a eliminar:");
        product.ShowInfo();

        Console.Write("\n¿Está seguro que desea eliminar este producto? (S/N): ");
        string confirmation = Console.ReadLine()?.ToUpper() ?? "";

        if (confirmation == "S" || confirmation == "SI")
        {
            bool removed = manager.RemoveProduct(code);

            if (removed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nProducto eliminado exitosamente");
                Console.WriteLine("Datos guardados en exports/products.json");
                Console.ResetColor();
            }
        }
        else
        {
            Console.WriteLine("\nEliminación cancelada");
        }
    }
}