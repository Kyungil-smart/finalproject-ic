using System.Collections.Generic;
using UnityEngine;

public interface IProjectListViewer
{
    IReadOnlyList<ProjectData> Projects { get; }
}
