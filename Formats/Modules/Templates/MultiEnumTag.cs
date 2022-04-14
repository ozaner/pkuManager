using pkuManager.Formats.Fields;
using System;
using System.Collections.Generic;

namespace pkuManager.Formats.Modules.Templates;

public interface MultiEnumTag_E
{
    protected void ExportMultiEnumTag<T>(IDictionary<T, IField<bool>> enumToFieldMap, HashSet<T> enums) where T : struct, Enum
    {
        foreach ((T e, IField<bool> field) in enumToFieldMap)
            if (field is not null)
                field.Value = enums.Contains(e);
    }
}