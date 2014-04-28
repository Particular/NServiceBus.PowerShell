namespace NServiceBus.PowerShell.Helpers
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text;
    using Microsoft.Win32;

    /// <summary>
    /// Registry Implementation that supports registry views for WOW32 and WOW64 for .Net 2 
    /// </summary>
    internal class RegistryHelper
    {
        int WOWOption;
        IntPtr RootKey;

        const int KEY_QUERY_VALUE = 0x0001;
        const int KEY_SET_VALUE = 0x0002;
        const int KEY_CREATE_SUB_KEY = 0x0004;
        const int KEY_ENUMERATE_SUB_KEYS = 0x0008;
        const int KEY_NOTIFY = 0x0010;
        const int KEY_CREATE_LINK = 0x0020;
        const int KEY_READ = ((STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY) & (~SYNCHRONIZE));
        const int KEY_WRITE = ((STANDARD_RIGHTS_WRITE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY) & (~SYNCHRONIZE));
        const int KEY_WOW64_64KEY = 0x0100;
        const int KEY_WOW64_32KEY = 0x0200;
        const int REG_OPTION_NON_VOLATILE = 0x0000;
        const int REG_OPTION_VOLATILE = 0x0001;
        const int REG_OPTION_CREATE_LINK = 0x0002;
        const int REG_OPTION_BACKUP_RESTORE = 0x0004;
        const int REG_NONE = 0;
        const int REG_SZ = 1;
        const int REG_EXPAND_SZ = 2;

        const int REG_BINARY = 3;
        const int REG_DWORD = 4;
        const int REG_DWORD_LITTLE_ENDIAN = 4;
        const int REG_DWORD_BIG_ENDIAN = 5;
        const int REG_LINK = 6;
        const int REG_MULTI_SZ = 7;
        const int REG_RESOURCE_LIST = 8;
        const int REG_FULL_RESOURCE_DESCRIPTOR = 9;
        const int REG_RESOURCE_REQUIREMENTS_LIST = 10;
        const int REG_QWORD = 11;
        const int READ_CONTROL = 0x00020000;
        const int SYNCHRONIZE = 0x00100000;

        const int STANDARD_RIGHTS_READ = READ_CONTROL;
        const int STANDARD_RIGHTS_WRITE = READ_CONTROL;

        const int SUCCESS = 0;
        const int FILE_NOT_FOUND = 2;
        const int ACCESS_DENIED = 5;
        const int INVALID_PARAMETER = 87;
        const int MORE_DATA = 234;
        const int NO_MORE_ENTRIES = 259;
        const int MARKED_FOR_DELETION = 1018;
        const int BUFFER_MAX_LENGTH = 2048;

        static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int) 0x80000001));
        static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int) 0x80000002));

        const int MaxKeyLength = 255;
        const int MaxValueLength = 16383;

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int sam, out IntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        static extern int RegEnumKey(IntPtr keyBase, int index, StringBuilder nameBuffer, int bufferLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int RegCloseKey(IntPtr hKey);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int RegEnumValue(IntPtr hKey, int dwIndex, StringBuilder lpValueName, ref int lpcchValueName, int lpReserved, int lpType, int lpData, int lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern int RegQueryValueEx(IntPtr hKey, String lpValueName, int[] lpReserved, ref int lpType, ref int lpData, ref int lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern int RegQueryValueEx(IntPtr hKey, String lpValueName, int[] lpReserved, ref int lpType, ref long lpData, ref int lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern int RegQueryValueEx(IntPtr hKey, String lpValueName, int[] lpReserved, ref int lpType, [Out] byte[] lpData, ref int lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern int RegQueryValueEx(IntPtr hKey, String lpValueName, int[] lpReserved, ref int lpType, [Out] char[] lpData, ref int lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern int RegSetValueEx(IntPtr hKey, String lpValueName, int reserved, RegistryValueKind dwType, byte[] lpData, int cbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern int RegSetValueEx(IntPtr hKey, String lpValueName, int reserved, RegistryValueKind dwType, ref int lpData, int cbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern int RegSetValueEx(IntPtr hKey, String lpValueName, int reserved, RegistryValueKind dwType, ref long lpData, int cbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern int RegSetValueEx(IntPtr hKey, String lpValueName, int reserved, RegistryValueKind dwType, String lpData, int cbData);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern int RegCreateKeyEx(IntPtr hKey, string lpSubKey, int reserved, String lpClass, int dwOptions, int samDesired, IntPtr lpSecurityAttributes, out IntPtr phkResult, out int lpdwDisposition);

        public static RegistryHelper LocalMachine(RegistryView regView)
        {
            var helper = new RegistryHelper
            {
                RootKey = HKEY_LOCAL_MACHINE,
                WOWOption = (int) regView
            };
            return helper;
        }

        public static RegistryHelper CurrentUser(RegistryView regView)
        {
            var helper = new RegistryHelper
            {
                RootKey = HKEY_CURRENT_USER,
                WOWOption = (int) regView
            };
            return helper;
        }

        public string[] GetSubKeyNames(string subKeyName)
        {
            var regKeyHandle = IntPtr.Zero;
            var keyNames = new ArrayList();
            try
            {
                if (RegOpenKeyEx(RootKey, subKeyName, 0, KEY_READ | WOWOption, out regKeyHandle) != 0)
                {
                    throw new Exception("Failed to open registry key");
                }
                var buffer = new StringBuilder(BUFFER_MAX_LENGTH);
                for (var index = 0;; index++)
                {
                    var result = RegEnumKey(regKeyHandle, index, buffer, buffer.Capacity);

                    if (result == SUCCESS)
                    {
                        keyNames.Add(buffer.ToString());
                        buffer.Length = 0;
                        continue;
                    }

                    if (result == NO_MORE_ENTRIES)
                    {
                        break;
                    }
                    throw new Win32Exception(result);
                }
                return (string[]) keyNames.ToArray(typeof(string));
            }
            finally
            {
                if (regKeyHandle != IntPtr.Zero)
                {
                    RegCloseKey(regKeyHandle);
                }
            }
        }

        public string[] GetValueNames(string subKeyName)
        {
            var regKeyHandle = IntPtr.Zero;
            var keyNames = new ArrayList();
            try
            {
                if (RegOpenKeyEx(RootKey, subKeyName, 0, KEY_READ | WOWOption, out regKeyHandle) != 0)
                {
                    throw new Exception("Failed to open registry key");
                }
                var buffer = new StringBuilder(256);

                for (var index = 0;; index++)
                {
                    var bufferSize = 256;
                    var result = RegEnumValue(regKeyHandle, index, buffer, ref bufferSize, 0, 0, 0, 0);
                    if (result == SUCCESS)
                    {
                        keyNames.Add(buffer.ToString());
                        buffer.Capacity = 256;
                        buffer.Length = 0;
                        continue;
                    }
                    if (result == NO_MORE_ENTRIES)
                    {
                        break;
                    }
                    throw new Win32Exception(result);
                }
                return (string[]) keyNames.ToArray(typeof(string));
            }
            finally
            {
                if (regKeyHandle != IntPtr.Zero)
                {
                    RegCloseKey(regKeyHandle);
                }
            }
        }

        public object ReadValue(string subKeyName, string valueName, object defaultValue, bool doNotExpand)
        {
            var data = defaultValue;
            var type = 0;
            var datasize = 0;
            var regKeyHandle = IntPtr.Zero;
            try
            {
                if (RegOpenKeyEx(RootKey, subKeyName, 0, KEY_READ | WOWOption, out regKeyHandle) != 0)
                {
                    throw new Exception("Failed to open registry key");
                }
                var ret = RegQueryValueEx(regKeyHandle, valueName, null, ref type, (byte[]) null, ref datasize);
                if (ret != 0)
                {
                    if (ret != MORE_DATA)
                    {
                        return data; //Error Return Default
                    }
                }
                else
                {
                    if (datasize < 0)
                    {
                        datasize = 0;
                    }
                    switch (type)
                    {
                        case REG_NONE:
                        case REG_DWORD_BIG_ENDIAN:
                        case REG_BINARY:
                        {
                            var blob = new byte[datasize];
                            RegQueryValueEx(regKeyHandle, valueName, null, ref type, blob, ref datasize);
                            data = blob;
                        }
                            break;
                        case REG_QWORD:
                        {
                            if (datasize > 8)
                            {
                                goto case REG_BINARY;
                            }
                            long blob = 0;
                            RegQueryValueEx(regKeyHandle, valueName, null, ref type, ref blob, ref datasize);
                            data = blob;
                        }
                            break;
                        case REG_DWORD:
                        {
                            if (datasize > 4)
                            {
                                goto case REG_QWORD;
                            }
                            var blob = 0;
                            RegQueryValueEx(regKeyHandle, valueName, null, ref type, ref blob, ref datasize);
                            data = blob;
                        }
                            break;

                        case REG_SZ:
                        {
                            var blob = new char[datasize/2];
                            RegQueryValueEx(regKeyHandle, valueName, null, ref type, blob, ref datasize);
                            if (blob.Length > 0 && blob[blob.Length - 1] == (char) 0)
                            {
                                data = new String(blob, 0, blob.Length - 1);
                            }
                            else
                            {
                                data = new String(blob);
                            }
                        }
                            break;
                        case REG_EXPAND_SZ:
                        {
                            var blob = new char[datasize/2];
                            RegQueryValueEx(regKeyHandle, valueName, null, ref type, blob, ref datasize);
                            if (blob.Length > 0 && blob[blob.Length - 1] == (char) 0)
                            {
                                data = new String(blob, 0, blob.Length - 1);
                            }
                            else
                            {
                                data = new String(blob);
                            }
                            if (!doNotExpand)
                                data = Environment.ExpandEnvironmentVariables((String) data);
                        }
                            break;
                        case REG_MULTI_SZ:
                        {
                            var blob = new char[datasize/2];

                            RegQueryValueEx(regKeyHandle, valueName, null, ref type, blob, ref datasize);

                            // Ensure String is null terminated
                            if (blob.Length > 0 && blob[blob.Length - 1] != (char) 0)
                            {
                                var newBlob = new char[checked(blob.Length + 1)];
                                for (var i = 0; i < blob.Length; i++)
                                {
                                    newBlob[i] = blob[i];
                                }
                                newBlob[newBlob.Length - 1] = (char) 0;
                                blob = newBlob;
                                blob[blob.Length - 1] = (char) 0;
                            }

                            IList<String> strings = new List<String>();
                            var cur = 0;
                            var len = blob.Length;

                            while (ret == 0 && cur < len)
                            {
                                var nextNull = cur;
                                while (nextNull < len && blob[nextNull] != (char) 0)
                                {
                                    nextNull++;
                                }

                                if (nextNull < len)
                                {
                                    if (nextNull - cur > 0)
                                    {
                                        strings.Add(new String(blob, cur, nextNull - cur));
                                    }
                                    else
                                    {
                                        if (nextNull != len - 1)
                                            strings.Add(String.Empty);
                                    }
                                }
                                else
                                {
                                    strings.Add(new String(blob, cur, len - cur));
                                }
                                cur = nextNull + 1;
                            }

                            data = new String[strings.Count];
                            strings.CopyTo((String[]) data, 0);
                        }
                            break;
                    }
                }
                return data;
            }
            finally
            {
                if (regKeyHandle != IntPtr.Zero)
                {
                    RegCloseKey(regKeyHandle);
                }
            }
        }

        public bool KeyExists(string subKeyName)
        {
            var regKeyHandle = IntPtr.Zero;
            try
            {
                
                if (RegOpenKeyEx(RootKey, subKeyName, 0, KEY_READ | WOWOption, out regKeyHandle) != 0)
                    return false;
                if (regKeyHandle == IntPtr.Zero)
                    return false;
                return true;
            }
            finally 
            {
                if (regKeyHandle != IntPtr.Zero)
                {
                    RegCloseKey(regKeyHandle);
                }
            }
        }

        public bool CreateSubkey(string subKeyName)
        {
            var regKeyHandle = IntPtr.Zero;

            var disposition = 0;
            var status = RegCreateKeyEx(RootKey, subKeyName, 0, null, 0, KEY_READ | KEY_WRITE | WOWOption, IntPtr.Zero, out regKeyHandle, out disposition);

            return !(status != 0 | regKeyHandle == IntPtr.Zero);
        }

        public bool WriteValue(string subKeyName, string valueName, object value, RegistryValueKind valueKind)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value", "value can't be null");
            }
            if (valueName != null && valueName.Length > MaxValueLength)
            {
                throw new ArgumentException("value name is invalid");
            }

            var regKeyHandle = IntPtr.Zero;
            try
            {

                var disposition = 0;
                var status = RegCreateKeyEx(RootKey, subKeyName, 0, null, 0, KEY_READ | KEY_WRITE | WOWOption, IntPtr.Zero, out regKeyHandle, out disposition);

                if (status != 0)
                {
                    throw new Win32Exception();
                }

                switch (valueKind)
                {
                    case RegistryValueKind.ExpandString:
                    case RegistryValueKind.String:
                    {
                        var data = value.ToString();
                        return RegSetValueEx(regKeyHandle, valueName, 0, valueKind, data, checked(data.Length * 2 + 2)) == 0;
                    }
                    case RegistryValueKind.Binary:
                    {
                        var dataBytes = (byte[]) value;
                        return RegSetValueEx(regKeyHandle, valueName, 0, RegistryValueKind.Binary, dataBytes, dataBytes.Length) == 0;
                    }
                    case RegistryValueKind.DWord:
                    {
                        var data = Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
                        return RegSetValueEx(regKeyHandle, valueName, 0, RegistryValueKind.DWord, ref data, 4) == 0;
                    }
                    case RegistryValueKind.QWord:
                    {
                        var data = Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
                        return RegSetValueEx(regKeyHandle, valueName, 0, RegistryValueKind.QWord, ref data, 8) == 0;
                    }
                    default:
                        throw new NotImplementedException(string.Format("RegistryKind {0} not supported", valueKind));
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to write value", ex);
            }
            finally
            {
                if (regKeyHandle != IntPtr.Zero)
                {
                    RegCloseKey(regKeyHandle);
                }
            }
        }

        public RegistryValueKind GetRegistryValueKind(string subKeyName, string valueName)
        {
            var regKeyHandle = IntPtr.Zero;
            try
            {
                if (RegOpenKeyEx(RootKey, subKeyName, 0, KEY_READ | WOWOption, out regKeyHandle) != 0)
                {
                    throw new Exception("Failed to open registry key");
                }
                var type = 0;
                var datasize = 0;
                var ret = RegQueryValueEx(regKeyHandle, valueName, null, ref type, (byte[]) null, ref datasize);
                if (ret != 0)
                    throw new Win32Exception(ret);
                if (!Enum.IsDefined(typeof(RegistryValueKind), type))
                {
                    return RegistryValueKind.Unknown;
                }
                return (RegistryValueKind) type;
            }
            finally
            {
                if (regKeyHandle != IntPtr.Zero)
                {
                    RegCloseKey(regKeyHandle);
                }
            }
        }
    }
}