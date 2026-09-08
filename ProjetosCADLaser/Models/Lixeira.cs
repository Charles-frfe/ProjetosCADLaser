using System;

namespace ProjetosCADLaser.Models
{
    public enum TipoRegistroLixeira { Piloto, TesteTextura }
    public sealed class RegistroLixeira { public int VersaoFormato { get; set; } public TipoRegistroLixeira Tipo { get; set; } public Guid Id { get; set; } public string Codigo { get; set; } public string Nome { get; set; } public string CaminhoOriginal { get; set; } public DateTimeOffset ExcluidoEm { get; set; } public string ExcluidoPor { get; set; } public string Computador { get; set; } public string Motivo { get; set; } public DateTimeOffset DisponivelAte { get; set; } public RegistroLixeira() { VersaoFormato = 1; Codigo = null; Nome = null; CaminhoOriginal = string.Empty; ExcluidoEm = DateTimeOffset.Now; ExcluidoPor = Environment.UserName; Computador = Environment.MachineName; Motivo = string.Empty; DisponivelAte = DateTimeOffset.Now.AddDays(7); } }
}
