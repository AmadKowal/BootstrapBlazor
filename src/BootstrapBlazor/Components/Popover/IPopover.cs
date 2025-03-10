﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

namespace BootstrapBlazor.Components;

/// <summary>
/// 
/// </summary>
public interface IPopover : ITooltip
{
    /// <summary>
    /// 获得/设置 显示文字
    /// </summary>
    public string? Content { get; set; }
}
