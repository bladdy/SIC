using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Entities;

public class FormularioModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Titulo { get; set; } = "";

    public List<PreguntaModel> Preguntas { get; set; } = new();
}

public class PreguntaModel
{
    public Guid Id { get; set; }

    public string Titulo { get; set; } = "";

    public TipoPregunta Tipo { get; set; }

    public bool Obligatoria { get; set; }

    public List<string> Opciones { get; set; } = new();

    public List<ReglaPreguntaModel> Reglas { get; set; } = new();
}
public class ReglaPreguntaModel
{
    public string ValorDisparador { get; set; } = "";

    public List<PreguntaModel> SubPreguntas { get; set; } = new();
}
public enum TipoPregunta
{
    Texto,
    Numero,
    SiNo,
    Lista,
    Fecha,
    Foto
}