// Decompiled with JetBrains decompiler
// Type: GPSoftware.Core.Dtos.IMySoftDeletedDto
// Assembly: GPSoftware.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FACC9EDE-6A86-449D-A829-9E0606380962
// Assembly location: C:\temp\GPSoftware.Core.dll

namespace GPSoftware.Core.Dtos {

    /// <summary>
    ///     Extension of <see cref="IMyDto"/> soft
    /// </summary>
    public interface IMySoftDeletedDto : IMyDto {
        bool ShowSoftDeleted { get; set; }
    }
}