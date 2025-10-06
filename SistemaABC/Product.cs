using System;
using System.Text.Json.Serialization;

/// <summary>
/// Representa un producto individual en el sistema WMS con soporte para análisis ABC.
/// Esta clase encapsula los datos esenciales del producto y proporciona validación integrada.
/// Se relaciona con ABCManager, que gestiona colecciones de productos y ejecuta la clasificación ABC.
/// </summary>
public class Product
{
    // Campos privados que almacenan los valores reales (backing fields)
    private string _code = string.Empty;
    private string _name = string.Empty;
    private int _movesPerMonth;
    private decimal _unitaryPrice;

    /// <summary>
    /// Código único del producto (SKU). Se normaliza a mayúsculas para evitar duplicados.
    /// Validación: No puede estar vacío.
    /// </summary>
    public string Code
    {
        get => _code;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El código del producto no puede estar vacío", nameof(Code));
            _code = value.ToUpper();
        }
    }

    /// <summary>
    /// Nombre descriptivo del producto para identificación humana.
    /// Validación: No puede estar vacío.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El nombre del producto no puede estar vacío", nameof(Name));
            _name = value;
        }
    }

    /// <summary>
    /// Movimientos mensuales del producto (frecuencia de salida del almacén).
    /// Validación: Debe ser >= 0. Es un factor clave en el análisis ABC.
    /// </summary>
    public int MovesPerMonth
    {
        get => _movesPerMonth;
        set
        {
            if (value < 0)
                throw new ArgumentException("Los movimientos mensuales no pueden ser negativos", nameof(MovesPerMonth));
            _movesPerMonth = value;
        }
    }

    /// <summary>
    /// Precio unitario del producto. Es un factor clave en el análisis ABC.
    /// Validación: Debe ser >= 0.
    /// </summary>
    public decimal UnitaryPrice
    {
        get => _unitaryPrice;
        set
        {
            if (value < 0)
                throw new ArgumentException("El precio unitario no puede ser negativo", nameof(UnitaryPrice));
            _unitaryPrice = value;
        }
    }

    /// <summary>
    /// Valor total calculado (MovesPerMonth * UnitaryPrice). 
    /// Es la métrica principal para la clasificación ABC y se recalcula automáticamente.
    /// No se persiste en JSON ya que es un valor derivado.
    /// </summary>
    public decimal TotalValue => _movesPerMonth * _unitaryPrice;

    /// <summary>
    /// Porcentaje acumulado del valor total del inventario.
    /// Asignado por ABCManager.ClassifyProducts() durante el análisis ABC.
    /// Se persiste en JSON para evitar recálculos innecesarios.
    /// </summary>
    public decimal AccumulatedPercentage { get; set; }

    /// <summary>
    /// Clasificación ABC del producto (A, B, o C).
    /// Valor por defecto: "C" (productos de bajo impacto).
    /// Asignado por ABCManager.ClassifyProducts() según los criterios de Pareto.
    /// Se persiste en JSON para mantener la clasificación entre sesiones.
    /// </summary>
    public string Classification { get; set; } = "C";

    /// <summary>
    /// Constructor completo para productos con datos históricos conocidos.
    /// Garantiza que el producto se cree en un estado válido mediante validación en las propiedades.
    /// </summary>
    public Product(string code, string name, int movesPerMonth, decimal unitaryPrice)
    {
        Code = code;
        Name = name;
        MovesPerMonth = movesPerMonth;
        UnitaryPrice = unitaryPrice;
    }

    /// <summary>
    /// Constructor para productos nuevos sin historial de movimientos.
    /// Inicializa MovesPerMonth en 0 (valor lógico para productos recién ingresados).
    /// </summary>
    public Product(string code, string name, decimal unitaryPrice)
        : this(code, name, 0, unitaryPrice)
    {
    }

    /// <summary>
    /// Constructor privado para deserialización JSON.
    /// Requerido por System.Text.Json para cargar objetos desde archivo.
    /// Solo accesible para el deserializador, no para código de aplicación.
    /// </summary>
    [JsonConstructor]
    private Product()
    {
        // El deserializador asignará valores a las propiedades automáticamente
    }

    /// <summary>
    /// Muestra la información del producto en formato legible para la consola.
    /// Utilizado por la interfaz de usuario (ProgramConsole) para mostrar detalles.
    /// </summary>
    public void ShowInfo()
    {
        Console.WriteLine($"[{Classification}] {Code} - {Name}");
        Console.WriteLine($"    Movimientos: {MovesPerMonth}\n    Precio: ${UnitaryPrice:N2}");
        Console.WriteLine($"    Valor Total: ${TotalValue:N2}\n    % Acum: {AccumulatedPercentage:N2}%");
    }

    /// <summary>
    /// Valida la integridad del producto después de la deserialización.
    /// Verifica que todos los campos esenciales tengan valores válidos.
    /// Utilizado por LoadTable durante la carga de datos para filtrar productos corruptos.
    /// </summary>
    /// <exception cref="InvalidOperationException">Cuando se detectan datos inválidos</exception>
    public void ValidateAfterDeserialization()
    {
        if (string.IsNullOrWhiteSpace(_code))
            throw new InvalidOperationException("El producto no tiene un código válido");
        
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvalidOperationException("El producto no tiene un nombre válido");
        
        if (_movesPerMonth < 0)
            throw new InvalidOperationException($"Movimientos mensuales negativos: {_movesPerMonth}");
        
        if (_unitaryPrice < 0)
            throw new InvalidOperationException($"Precio unitario negativo: {_unitaryPrice}");
        
        if (_movesPerMonth == 0 && _unitaryPrice == 0)
            throw new InvalidOperationException("Producto sin valor ni movimientos (datos incompletos)");
    }
}