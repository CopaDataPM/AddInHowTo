using System;
using Scada.AddIn.Contracts;
using DriverCommon;

namespace POZYTON_API
{
    /// <summary>
    /// Description of Editor Wizard Extension.
    /// </summary>
    [AddInExtension("POZYTON_API", "Test POZYTON API, Import and Export", "Drivers API/Export/Import")]
    public class EditorWizardExtension : IEditorWizardExtension
    {
        private Log _log;
        private DriverContext _driverContext;

        const string DriverIdent = "POZYTON";
        const string DriverName = "POZYTON";
        const string XmlSuffixBefore = "before";
        const string XmlSuffixAfter = "after";

        #region IEditorWizardExtension implementation

        public void Run(IEditorApplication context, IBehavior behavior)
        {
            _log = new Log(context, DriverIdent);

            try
            {
                _driverContext = new DriverContext(context, _log, DriverName, false);

                // enter your code which should be executed when starting the SCADA Editor Wizard

                _log.Message("begin test");

                _driverContext.Export(XmlSuffixBefore);

                if (_driverContext.OpenDriver(10))
                {
                    _driverContext.DumpNodeInfo("DrvConfig.Options");

                    _driverContext.ModifyCommonProperties();
                    _driverContext.ModifyCOMProperties();
          
                    ModifyConnections();
          
                    _driverContext.CloseDriver();

                    _driverContext.Export(XmlSuffixAfter);
                    _driverContext.Import(XmlSuffixBefore);
                }

                _log.Message("end test");
            }
            catch (Exception ex)
            {
                _log.ExpectionMessage($"An exception has been thrown: {ex.Message}", ex);
                throw;
            }
        }

    private void ModifyConnections()
    {
      _log.FunctionEntryMessage("modify connections");

      string[] propItems;
      uint connCount;
      _driverContext.GetNodeInfo("DrvConfig.Connections", out propItems, out connCount);

      uint idxI;
      for (idxI = 0; idxI < connCount; idxI++)
      {
        ModifyConnection(idxI);
      }

      _log.FunctionExitMessage();

      // add a new connection (not using an index)
      if (_driverContext.AddNode("DrvConfig.Connections"))
      {
        // this connection remains with default values

        idxI += 1;
        // add a new connection - uses an index
        if (_driverContext.AddNode("DrvConfig.Connections[" + idxI.ToString() + "]"))
        {
          // this connection gets the minimum information necessary to be accepted by the driver
          ModifyConnection(idxI);
        }
      }
    }

    private void ModifyConnection(uint connIndex)
    {
      string connNamePrefix;
      string connIndexString = connIndex.ToString();
      connNamePrefix = "DrvConfig.Connections[" + connIndexString + "].";

      connIndex = connIndex + 1;

      _log.FunctionEntryMessage($"modify {connIndex}. connection");

      _driverContext.SetUnsignedProperty(connNamePrefix + "NetAdress", 0, 0, 999, true);
      _driverContext.SetStringProperty(connNamePrefix + "ConnectionName", "DeviceA" + connIndexString, true);
      _driverContext.SetStringProperty(connNamePrefix + "SerialNumber", "333.7777777" + connIndexString, true);

      _log.FunctionExitMessage();
    }

    #endregion
  }

}
