using System.Collections.Generic;

namespace DataTables.NetStandard.Enhanced.Filters
{
    public abstract class BaseFilter : IColumnFilter
    {
        /// <summary>
        /// Gets the type of the filter. Supported values depend on the yadcf library version.
        /// </summary>
        public abstract string FilterType { get; }

        /// <summary>
        /// Gets the filter delay in milliseconds. Is used to throttle filtering on some inputs.
        /// </summary>
        public virtual int FilterDelay { get; } = 350;

        /// <summary>
        /// Gets or sets the filter reset button text. Null can be used to disable the button.
        /// </summary>
        public string FilterResetButtonText { get; set; }

        /// <summary>
        /// Gets or sets the style class used for the filter HTML element.
        /// </summary>
        public string StyleClass { get; set; }


        public virtual FilterOptions GetFilterOptions(int columnIndex)
        {
            return GetFilterOptions(columnIndex, null);
        }

        protected FilterOptions GetFilterOptions(int columnIndex, IDictionary<string, dynamic> additionalOptions)
        {
            if (additionalOptions == null)
            {
                additionalOptions = new Dictionary<string, dynamic>();
            }

            return new FilterOptions
            {
                ColumnNumber = columnIndex,
                Type = FilterType,
                FilterDelay = FilterDelay,
                FilterResetButtonText = FilterResetButtonText,
                StyleClass = StyleClass,
                AdditionalOptions = additionalOptions,
            };
        }
    }
}
