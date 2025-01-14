﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

namespace BootstrapBlazor.Components;

/// <summary>
/// BootstrapTooltip 组件
/// </summary>
public partial class Tooltip : ITooltip, IAsyncDisposable
{
    /// <summary>
    /// 弹窗位置字符串
    /// </summary>
    protected string? PlacementString => Placement == Placement.Auto ? null : Placement.ToDescriptionString();

    /// <summary>
    /// 弹窗位置字符串
    /// </summary>
    protected string? SanitizeString => Sanitize ? null : "false";

    /// <summary>
    /// 弹窗位置字符串
    /// </summary>
    protected string? HtmlString => IsHtml ? "true" : null;

    /// <summary>
    /// data-bs-toggle value default value is tooltip/popover
    /// </summary>
    [NotNull]
    protected string? ToggleString { get; set; }

    /// <summary>
    /// component class
    /// </summary>
    protected string? ClassString => CssBuilder.Default()
        .AddClassFromAttributes(AdditionalAttributes)
        .Build();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Parameter]
    public string? Delay { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Parameter]
    public string? Selector { get; set; }

    /// <summary>
    /// 获得/设置 显示内容
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// 获得/设置 显示文字是否为 Html 默认为 false
    /// </summary>
    [Parameter]
    public bool IsHtml { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Parameter]
    public bool Sanitize { get; set; } = true;

    /// <summary>
    /// 获得/设置 位置 默认为 Placement.Top
    /// </summary>
    [Parameter]
    public Placement Placement { get; set; } = Placement.Top;

    /// <summary>
    /// 获得/设置 自定义样式 默认 null
    /// </summary>
    /// <remarks>由 data-bs-custom-class 实现</remarks>
    [Parameter]
    public string? CustomClass { get; set; }

    /// <summary>
    /// 获得/设置 触发方式 可组合 click focus hover 默认为 focus hover
    /// </summary>
    [Parameter]
    public string? Trigger { get; set; }

    /// <summary>
    /// 获得/设置 子组件
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        ToggleString = "tooltip";
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        Trigger ??= "focus hover";
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="firstRender"></param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        await JSInvokeAsync();
    }

    /// <summary>
    /// 设置参数方法
    /// </summary>
    public void SetParameters(string title, Placement placement = Placement.Auto, string? trigger = null, string? customClass = null, bool? isHtml = null, bool? sanitize = null, string? delay = null, string? selector = null)
    {
        Title = title;
        if (placement != Placement.Auto) Placement = placement;
        if (!string.IsNullOrEmpty(trigger)) Trigger = trigger;
        if (!string.IsNullOrEmpty(customClass)) CustomClass = customClass;
        if (isHtml.HasValue) IsHtml = isHtml.Value;
        if (sanitize.HasValue) Sanitize = sanitize.Value;
        if (!string.IsNullOrEmpty(delay)) Delay = delay;
        if (!string.IsNullOrEmpty(selector)) Selector = selector;
        StateHasChanged();
    }

    /// <summary>
    /// JavaScript invoke
    /// </summary>
    /// <returns></returns>
    protected virtual async ValueTask JSInvokeAsync()
    {
        if (!string.IsNullOrEmpty(Title))
        {
            await JSRuntime.InvokeVoidAsync(identifier: "bb.Tooltip.init", $"#{Id}", Title);
        }
    }

    /// <summary>
    /// DisposeAsync 方法
    /// </summary>
    /// <param name="disposing"></param>
    /// <returns></returns>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await JSRuntime.InvokeVoidAsync(identifier: "bb.Tooltip.dispose", $"#{Id}");
        }
    }

    /// <summary>
    /// DisposeAsync 方法
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }
}
