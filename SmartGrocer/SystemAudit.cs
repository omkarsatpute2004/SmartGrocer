using System;
using System.IO;
using System.Linq;

namespace SmartGrocer.Audit
{
    public class AuditRunner
    {
        public static void Run(string[] args)
        {
            Console.WriteLine("--- SMARTGROCER SYSTEM AUDIT ---");
            
            VerifyCurrency();
            VerifyCategoryLogic();
            
            Console.WriteLine("--- AUDIT COMPLETE ---");
        }

        private static void VerifyCurrency()
        {
            Console.WriteLine("[Checking Currency Symbols...]");
            string[] views = { 
                "Views/Owner/Dashboard.cshtml", 
                "Views/Owner/Products.cshtml", 
                "Views/Owner/Sales.cshtml", 
                "Views/Employee/Stock.cshtml" 
            };

            foreach (var view in views)
            {
                if (File.Exists(view))
                {
                    var content = File.ReadAllText(view);
                    bool hasDollar = content.Contains("$");
                    bool hasRupee = content.Contains("₹");
                    
                    Console.WriteLine($"  - {view}: {(hasRupee ? "✅ Rupee Found" : "❌ Rupee Missing")} {(hasDollar ? "❌ Dollar Still Exists" : "✅ Dollar Removed")}");
                }
            }
        }

        private static void VerifyCategoryLogic()
        {
            Console.WriteLine("[Checking Category Delete Logic...]");
            string controllerPath = "Controllers/OwnerController.cs";
            
            if (File.Exists(controllerPath))
            {
                var content = File.ReadAllText(controllerPath);
                bool hasSafetyCheck = content.Contains("AnyAsync(p => p.CategoryId == id)");
                
                Console.WriteLine($"  - OwnerController Logic: {(hasSafetyCheck ? "✅ Prevention Logic Found" : "❌ Prevention Logic Missing")}");
            }
        }
    }
}
