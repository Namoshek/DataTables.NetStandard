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
        /// Gets or sets the data used to initialize the filter. For filters of type `select`,
        /// this can be a range of options for example.
        /// 
        /// Note: If, instead of a list of strings, a list of options is passed, they will be
        /// serialized as-they-are.
        /// </summary>
        public virtual IList<object> Data { get; set; } = null;

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


        public object GetFilterOptions(int columnIndex)
        {
            return GetFilterOptions(columnIndex, null);
        }

        protected object GetFilterOptions(int columnIndex, IDictionary<string, dynamic> additionalOptions)
        {
            if (additionalOptions == null)
            {
                additionalOptions = new Dictionary<string, dynamic>();
            }

            return new FilterOptions
            {
                ColumnNumber = columnIndex,
                Type = FilterType,
                Data = Data,
                FilterDelay = FilterDelay,
                FilterResetButtonText = FilterResetButtonText,
                StyleClass = StyleClass,
                AdditionalOptions = additionalOptions,
            };
        }
    }
}
