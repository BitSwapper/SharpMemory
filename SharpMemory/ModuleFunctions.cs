using System.Diagnostics;

namespace SharpMemory;
public class ModuleFunctions
{
    SharpMem SharpMem { get; init; }
    public ModuleFunctions(SharpMem sharpMem)
    {
        SharpMem = sharpMem;
    }
    public long GetModuleAddress(string moduleName) => (long)GetModule(moduleName).BaseAddress;

    public ProcessModule GetModule(string moduleName) => GetModuleAndDuplicates(moduleName).First();

    public ProcessModule[] GetModuleAndDuplicates(string moduleName) => GetAllModules().Where(x => x.ModuleName.Equals(moduleName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
    
    public ProcessModule[] GetAllModules()
    {
        try
        {
            List<ProcessModule> list = new();
            for(int i = 0; i < SharpMem.Process.Modules.Count; i++)
                if(SharpMem.Process.Modules[i] != null)
                    list.Add(SharpMem.Process.Modules[i]);
            return list.ToArray();
        }
        catch(Exception ex)
        {
            Debug.WriteLine("ModuleSearchException: " + ex.Message);
        }
        return null;
    }
}
