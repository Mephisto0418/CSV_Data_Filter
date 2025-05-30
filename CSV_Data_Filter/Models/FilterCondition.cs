namespace CSV_Data_Filter.Models
{
    public class FilterCondition
    {
        public FilterCondition()
        {
            ColumnName = string.Empty;
            Value = string.Empty;
            DateFormat = "yyyy-MM-dd";
        }

        public string ColumnName { get; set; }
        public FilterOperator Operator { get; set; }
        public string Value { get; set; }
        public string DateFormat { get; set; }

        public override string ToString()
        {
            string operatorDesc = "";
            switch (Operator)
            {
                case FilterOperator.Equals: operatorDesc = "等於"; break;
                case FilterOperator.NotEquals: operatorDesc = "不等於"; break;
                case FilterOperator.Contains: operatorDesc = "包含"; break;
                case FilterOperator.NotContains: operatorDesc = "不包含"; break;
                case FilterOperator.StartsWith: operatorDesc = "開頭為"; break;
                case FilterOperator.EndsWith: operatorDesc = "結尾為"; break;
                case FilterOperator.GreaterThan: operatorDesc = "大於"; break;
                case FilterOperator.LessThan: operatorDesc = "小於"; break;
                case FilterOperator.GreaterThanOrEqual: operatorDesc = "大於或等於"; break;
                case FilterOperator.LessThanOrEqual: operatorDesc = "小於或等於"; break;
                case FilterOperator.DateGreaterThan: operatorDesc = "日期大於"; break;
                case FilterOperator.DateLessThan: operatorDesc = "日期小於"; break;
                case FilterOperator.DateGreaterThanOrEqual: operatorDesc = "日期大於或等於"; break;
                case FilterOperator.DateLessThanOrEqual: operatorDesc = "日期小於或等於"; break;
            }

            return $"{ColumnName} {operatorDesc} {Value}";
        }
    }

    public enum FilterOperator
    {
        Equals,
        NotEquals,
        Contains,
        NotContains,
        StartsWith,
        EndsWith,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        DateGreaterThan,
        DateLessThan,
        DateGreaterThanOrEqual,
        DateLessThanOrEqual
    }
}