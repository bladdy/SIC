namespace SIC.Shared.DTOs
{
    public class BoletaInvitacionDto
    {
        public string NombreInvitado { get; set; } = string.Empty;
        public string NombreEvento { get; set; } = string.Empty;
        public string SubNombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Hora { get; set; } = string.Empty;
        public string Lugar { get; set; } = string.Empty;
        public int CantidadPersonas { get; set; }
        public int Adultos { get; set; }
        public int Jovenes { get; set; }
        public int Niños { get; set; }
        public string MesaAsignada { get; set; } = string.Empty;
        public string CodigoQr { get; set; } = string.Empty;
        public string CoverImageBytes { get; set; } = string.Empty;
        public List<string> Guests { get; set; } = [];
        public bool IsIndividualAssignment { get; set; }
        public List<string> GuestsWithMesa { get; set; } = [];
    }
}