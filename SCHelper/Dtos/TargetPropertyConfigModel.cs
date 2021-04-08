using SCHelper.Exceptions;

namespace SCHelper.Dtos
{
    public class TargetPropertyConfigModel
    {
        public string Property { get; set; }
        public double Impact { get; set; }
        public double Min { get; set; }
        public double Best { get; set; }
    }

    public static class LimitationConfigModelExtensions
    {
        public static TargerPropertyModel ToDomainModel(this TargetPropertyConfigModel data)
            => new TargerPropertyModel(
                Property: data.Property ?? throw new DataValidationException($"{nameof(data.Property)} is Null"),
                Impact: 0 < data.Impact && data.Impact < 1
                    ? data.Impact
                    : throw new DataValidationException($"{nameof(data.Impact)} should be in range (0; 1)"),
                Min: data.Min,
                Best: data.Best
            );
    }
}
