using System;
using System.Collections.Generic;

namespace Taller.Entidades;

public partial class Vehiculo
{
    public int VehiculoId { get; set; }

    public string Placa { get; set; } = null!;

    public string Marca { get; set; } = null!;

    public string Modelo { get; set; } = null!;

    public int Anio { get; set; }

    public int ClienteId { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;
}
