using Perspex.Controls;
using System;

namespace Mabiavalon
{
    internal class ContainerCustomizations
    {
        private readonly Func<DockItem> _getContainerForItemOverride;
        private readonly Action<Control, object> _prepareContainerForItemOverride;
        private readonly Action<Control, object> _clearingContainerForItemOverride;

        public ContainerCustomizations(Func<DockItem> getContainerForItemOverride = null, Action<Control, object> prepareContainerForItemOverride = null, Action<Control, object> clearingContainerForItemOverride = null)
        {
            _getContainerForItemOverride = getContainerForItemOverride;
            _prepareContainerForItemOverride = prepareContainerForItemOverride;
            _clearingContainerForItemOverride = clearingContainerForItemOverride;
        }

        public Func<DockItem> GetContainerForItemOverride
        {
            get { return _getContainerForItemOverride; }
        }

        public Action<Control, object> PrepareContainerForItemOverride
        {
            get { return _prepareContainerForItemOverride; }
        }

        public Action<Control, object> ClearingContainerForItemOverride
        {
            get { return _clearingContainerForItemOverride; }
        }
    }
}
