using System.Diagnostics;

namespace SharpMemory;
public class ModuleFunctions
{
    public long GetModuleAddress(string moduleName) => (long)GetModule(moduleName).BaseAddress;

    public ProcessModule GetModule(string moduleName) => GetModuleInstances(moduleName).First();

    public ProcessModule[] GetModuleInstances(string moduleName) => GetAllModules().Where(x => x.ModuleName.Equals(moduleName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
    
    public ProcessModule[] GetAllModules()
    {
        try
        {
            List<ProcessModule> list = new();
            for(int i = 0; i < SharpMem.Inst.Process.Modules.Count; i++)
                if(SharpMem.Inst.Process.Modules[i] != null)
                    list.Add(SharpMem.Inst.Process.Modules[i]);
            return list.ToArray();
        }
        catch(Exception ex)
        {
            Debug.WriteLine("ModuleSearchException: " + ex.Message);
        }
        return null;
    }
}
