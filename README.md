Sistema WMS con Análisis ABC

Sistema de gestión de inventario para Warehouse Management System (WMS) que implementa el análisis ABC basado en la regla de Pareto (80/20). Clasifica automáticamente productos según su valor e importancia, optimizando la gestión de recursos en almacenes.

Características Principales

    Clasificación ABC Automática: Productos categorizados como A (críticos), B (importantes) y C (bajo impacto)
    Persistencia en JSON: Todos los datos se guardan automáticamente en exports/products.json
    Validación Robusta: Múltiples capas de validación garantizan integridad de datos
    Interfaz de Consola Intuitiva: Menú CRUD completo con retroalimentación visual clara
    Resistente a Errores: Recupera automáticamente de archivos JSON corruptos o editados manualmente
    Reclasificación Inteligente: Solo reclasifica cuando cambian campos relevantes (precio o movimientos)

Metodología ABC (Regla de Pareto)

El sistema clasifica productos según su *valor total mensual (Movimientos × Precio Unitario):

    Categoría A (≤80% acumulado): Productos críticos - Alta prioridad
        Ubicación: Cerca de zona de envío
        Control: Conteo diario, alta precisión
    Categoría B (≤95% acumulado): Productos importantes - Media prioridad
        Ubicación: Zona intermedia
        Control: Conteo semanal
    Categoría C (>95% acumulado): Productos de bajo impacto - Baja prioridad
        Ubicación: Zona alejada
        Control: Conteo mensual

Requisitos

    .NET 9.0
    IDE recomendado: Visual Studio Code

Instalación y Uso

    Clona el repositorio

    git clone https://github.com/AlexRiveraSDE/ABCTable.git
    cd sistema-wms-abc
