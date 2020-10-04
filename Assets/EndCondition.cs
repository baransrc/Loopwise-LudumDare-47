using UnityEngine;

public enum ValueType
{
    Int,
    Bool,
}

public enum SpecialEndConditionName
{
    PlayerScore,
    GameIsPaused,
    None,
}

public enum ConditionType
{
    GreaterThan,
    EqualTo,
    NotEqualTo,
    SmallerThan,
    GreaterThanOrEqualTo,
    SmallerThanOrEqualTo
}

[System.Serializable]
public class EndCondition
{
    public string name;
    public ConditionType type;
    public int value;
    public ValueType valueType;
    public int valueToRelate;
    public SpecialEndConditionName specialEndConditionName;

    private string GetOperatorByConditionType()
    {
        switch (type)
        {
            default:
                return "==";
            case ConditionType.GreaterThan:
                return ">";
            case ConditionType.EqualTo:
                return "==";
            case ConditionType.SmallerThan:
                return "<";
            case ConditionType.GreaterThanOrEqualTo:
                return ">=";
            case ConditionType.SmallerThanOrEqualTo:
                return "<=";
            case ConditionType.NotEqualTo:
                return "!=";
        }
    }

    public bool DoesEndConditionMet()
    {
        switch (type)
        {
            default:
                return value == valueToRelate;
            case ConditionType.GreaterThan:
                return value > valueToRelate;
            case ConditionType.EqualTo:
                return value == valueToRelate;
            case ConditionType.SmallerThan:
                return value < valueToRelate;
            case ConditionType.GreaterThanOrEqualTo:
                return value >= valueToRelate;
            case ConditionType.SmallerThanOrEqualTo:
                return value <= valueToRelate;
            case ConditionType.NotEqualTo:
                return value != valueToRelate;
        }
    }

    public string GetValue()
    {
        return valueType == ValueType.Bool ? (value != 0).ToString().ToLower() : value.ToString();
    }

    public string GetValueToRelate()
    {
        return valueType == ValueType.Bool ? (valueToRelate != 0).ToString().ToLower() : valueToRelate.ToString();
    }

    
    public string GetCommentText()
    {
        return name + ": " + GetValue();
    }
    
    public string GetConditionText()
    {
        return name + " " + GetOperatorByConditionType() + " " + GetValueToRelate();
    }

}
