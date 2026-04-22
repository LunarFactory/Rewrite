public enum ModifierType
{
    Flat,
    Percent,
} // 고정 수치(Flat) 증가냐, 퍼센트(%) 증가냐

public class StatModifier
{
    public string Name;
    public float Value; // 증가량
    public ModifierType Type; // 타입
    public object Source; // 이 버프를 준 주체 (아이템 이름 등)

    public StatModifier(string name, float value, ModifierType type, object source)
    {
        Name = name;
        Value = value;
        Type = type;
        Source = source;
    }
}
