﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using Microsoft.Extensions.Localization;

namespace BootstrapBlazor.Components;

/// <summary>
/// Select 组件实现类
/// </summary>
/// <typeparam name="TValue"></typeparam>
public partial class Select<TValue> : ISelect
{
    private ElementReference SelectElement { get; set; }

    private JSInterop<Select<TValue>>? Interop { get; set; }

    [Inject]
    [NotNull]
    private SwalService? SwalService { get; set; }

    /// <summary>
    /// 获得 样式集合
    /// </summary>
    private string? ClassString => CssBuilder.Default("select dropdown")
        .AddClassFromAttributes(AdditionalAttributes)
        .Build();

    /// <summary>
    /// 获得 样式集合
    /// </summary>
    private string? InputClassString => CssBuilder.Default("form-select form-control")
        .AddClass($"border-{Color.ToDescriptionString()}", Color != Color.None && !IsDisabled && !IsValid.HasValue)
        .AddClass($"border-success", IsValid.HasValue && IsValid.Value)
        .AddClass($"border-danger", IsValid.HasValue && !IsValid.Value)
        .AddClass(CssClass).AddClass(ValidCss)
        .Build();

    /// <summary>
    /// 获得 样式集合
    /// </summary>
    private string? AppendClassString => CssBuilder.Default("form-select-append")
        .AddClass($"text-{Color.ToDescriptionString()}", Color != Color.None && !IsDisabled && !IsValid.HasValue)
        .AddClass($"text-success", IsValid.HasValue && IsValid.Value)
        .AddClass($"text-danger", IsValid.HasValue && !IsValid.Value)
        .Build();

    /// <summary>
    /// 设置当前项是否 Active 方法
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private string? ActiveItem(SelectedItem item) => CssBuilder.Default("dropdown-item")
        .AddClass("active", () => item.Value == CurrentValueAsString)
        .AddClass("disabled", item.IsDisabled)
        .Build();

    /// <summary>
    /// Razor 文件中 Options 模板子项
    /// </summary>
    [NotNull]
    private List<SelectedItem>? Childs { get; set; }

    /// <summary>
    /// 获得/设置 右侧下拉箭头图标 默认 fa-solid fa-angle-up
    /// </summary>
    [Parameter]
    [NotNull]
    public string? DropdownIcon { get; set; }

    /// <summary>
    /// 获得/设置 搜索文本发生变化时回调此方法
    /// </summary>
    [Parameter]
    [NotNull]
    public Func<string, IEnumerable<SelectedItem>>? OnSearchTextChanged { get; set; }

    /// <summary>
    /// 获得/设置 选中候选项后是否自动清空搜索框内容 默认 false 不清空
    /// </summary>
    [Parameter]
    public bool AutoClearSearchText { get; set; }

    /// <summary>
    /// 获得 PlaceHolder 属性
    /// </summary>
    [Parameter]
    public string? PlaceHolder { get; set; }

    /// <summary>
    /// 获得/设置 选项模板支持静态数据
    /// </summary>
    [Parameter]
    public RenderFragment? Options { get; set; }

    /// <summary>
    /// 获得/设置 显示部分模板 默认 null
    /// </summary>
    [Parameter]
    public RenderFragment<SelectedItem?>? DisplayTemplate { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<Select<TValue>>? Localizer { get; set; }

    [NotNull]
    private List<SelectedItem>? DataSource { get; set; }

    /// <summary>
    /// 获得 input 组件 Id 方法
    /// </summary>
    /// <returns></returns>
    protected override string? RetrieveId() => InputId;

    /// <summary>
    /// 获得/设置 Select 内部 Input 组件 Id
    /// </summary>
    private string? InputId => $"{Id}_input";

    /// <summary>
    /// OnInitialized 方法
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        OnSearchTextChanged ??= text => Items.Where(i => i.Text.Contains(text, StringComparison));
        Childs = new List<SelectedItem>();
    }

    /// <summary>
    /// OnParametersSet 方法
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        Items ??= Enumerable.Empty<SelectedItem>();
        PlaceHolder ??= Localizer[nameof(PlaceHolder)];
        DropdownIcon ??= "fa-solid fa-angle-up";

        // 内置对枚举类型的支持
        var t = NullableUnderlyingType ?? typeof(TValue);
        if (!Items.Any() && t.IsEnum())
        {
            var item = NullableUnderlyingType == null ? "" : PlaceHolder;
            Items = typeof(TValue).ToSelectList(string.IsNullOrEmpty(item) ? null : new SelectedItem("", item));
        }
    }

    private void ResetSelectedItem()
    {
        if (string.IsNullOrEmpty(SearchText))
        {
            DataSource = Items.ToList();
            DataSource.AddRange(Childs);
        }
        else
        {
            DataSource = OnSearchTextChanged(SearchText).ToList();
        }

        SelectedItem = DataSource.FirstOrDefault(i => i.Value.Equals(CurrentValueAsString, StringComparison))
            ?? DataSource.FirstOrDefault(i => i.Active)
            ?? DataSource.FirstOrDefault();

        // 检查 Value 值是否在候选项中存在
        // Value 不等于 选中值即不存在
        if (!string.IsNullOrEmpty(SelectedItem?.Value) && CurrentValueAsString != SelectedItem.Value)
        {
            CurrentValueAsString = SelectedItem.Value;
        }
    }

    /// <summary>
    /// OnAfterRenderAsync 方法
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            Interop ??= new JSInterop<Select<TValue>>(JSRuntime);
            await Interop.InvokeVoidAsync(this, SelectElement, "bb_select", nameof(ConfirmSelectedItem));

            // 选项值不为 null 后者 string.Empty 时触发一次 OnSelectedItemChanged 回调
            if (SelectedItem != null && OnSelectedItemChanged != null && !string.IsNullOrEmpty(SelectedItem.Value))
            {
                await OnSelectedItemChanged.Invoke(SelectedItem);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    [JSInvokable]
    public async Task ConfirmSelectedItem(int index)
    {
        var ds = string.IsNullOrEmpty(SearchText)
            ? DataSource
            : OnSearchTextChanged.Invoke(SearchText);
        var item = ds.ElementAt(index);
        await OnClickItem(item);
        StateHasChanged();
    }

    /// <summary>
    /// 下拉框选项点击时调用此方法
    /// </summary>
    private async Task OnClickItem(SelectedItem item)
    {
        var ret = true;
        if (OnBeforeSelectedItemChange != null)
        {
            ret = await OnBeforeSelectedItemChange(item);
            if (ret)
            {
                // 返回 True 弹窗提示
                var option = new SwalOption()
                {
                    Category = SwalCategory,
                    Title = SwalTitle,
                    Content = SwalContent
                };
                if (!string.IsNullOrEmpty(SwalFooter))
                {
                    option.ShowFooter = true;
                    option.FooterTemplate = builder => builder.AddContent(0, SwalFooter);
                }
                ret = await SwalService.ShowModal(option);
            }
            else
            {
                // 返回 False 直接运行
                ret = true;
            }
        }
        if (ret)
        {
            if (IsPopover)
            {
                await JSRuntime.InvokeVoidAsync(identifier: "bb.Dropdown.invoke", SelectElement, "hide");
            }
            await ItemChanged(item);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task ItemChanged(SelectedItem item)
    {
        item.Active = true;
        SelectedItem = item;
        CurrentValueAsString = item.Value;

        // 触发 SelectedItemChanged 事件
        if (OnSelectedItemChanged != null)
        {
            await OnSelectedItemChanged.Invoke(SelectedItem);
        }

        if (AutoClearSearchText)
        {
            SearchText = string.Empty;
        }
    }

    /// <summary>
    /// 添加静态下拉项方法
    /// </summary>
    /// <param name="item"></param>
    public void Add(SelectedItem item) => Childs.Add(item);
}
