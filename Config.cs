namespace KadAntiKOS;

public sealed class Config
{
    public bool Debug { get; set; } = false;
    public bool EnableGrace { get; set; } = false;
    public int GracePeriod { get; set; } = 5;
    public float GraceDamageResistance { get; set; } = 0.7f;
    public float CuffDamageResistance { get; set; } = 0.4f;
    public float DisarmMaxDistance { get; set; } = 20;
    public float UncuffMaxDistance { get; set; } = 13;
}
