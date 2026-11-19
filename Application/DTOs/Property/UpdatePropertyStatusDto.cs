using Domain.Enums;
using System.ComponentModel.DataAnnotations;

public class UpdatePropertyStatusDto
{
    [EnumDataType(typeof(PropertyStatus))]
    public PropertyStatus Status { get; set; }
}