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

        /// <summary>
        /// Gets or sets the additional filter options. The options are passed directly to the yadcf library.
        /// </summary>
        public IDictionary<string, dynamic> AdditionalOptions = new Dictionary<string, dynamic>();


        public virtual FilterOptions GetFilterOptions(int columnIndex)
        {
            return GetFilterOptions(columnIndex, null);
        }

        protected FilterOptions GetFilterOptions(int columnIndex, IDictionary<string, dynamic> additionalOptions)
        {
            var additionalOptionsClone = new Dictionary<string, dynamic>(AdditionalOptions);

            if (additionalOptions != null)
            {
                foreach (var option in additionalOptions)
                {
                    additionalOptionsClone.Add(option.Key, option.Value);
                }
            }

            return new FilterOptions
            {
                ColumnNumber = columnIndex,
                Type = FilterType,
                FilterDelay = FilterDelay,
                FilterResetButtonText = FilterResetButtonText,
                StyleClass = StyleClass,
                AdditionalOptions = additionalOptionsClone,
            };
        }
    }
}
