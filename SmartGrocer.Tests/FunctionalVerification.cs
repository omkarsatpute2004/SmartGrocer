using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SmartGrocer.Tests
{
    [TestClass]
    public class FunctionalVerification
    {
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "SmartGrocer"));

        [TestMethod]
        public void Verify_Currency_Symbols_In_Views()
        {
            Console.WriteLine("Verifying Indian Rupee symbol conversion...");
            string viewsDir = Path.Combine(_projectDir, "Views");
            
            // Files we expect to have Rupee symbol
            string[] filesToVerify = {
                Path.Combine(viewsDir, "Owner", "Dashboard.cshtml"),
                Path.Combine(viewsDir, "Owner", "Products.cshtml"),
                Path.Combine(viewsDir, "Owner", "Sales.cshtml"),
                Path.Combine(viewsDir, "Employee", "Stock.cshtml")
            };

            foreach (var file in filesToVerify)
            {
                Assert.IsTrue(File.Exists(file), $"View file missing: {file}");
                var content = File.ReadAllText(file);
                
                // Assert $ is gone and ₹ is present
                Assert.IsFalse(content.Contains("$"), $"Dollar symbol still exists in {file}");
                Assert.IsTrue(content.Contains("₹"), $"Indian Rupee symbol missing in {file}");
                Console.WriteLine($"✅ Verified: {Path.GetFileName(file)} uses ₹");
            }
        }

        [TestMethod]
        public void Verify_Category_Deletion_Safety_Logic()
        {
            Console.WriteLine("Verifying Category Delete safety logic in Controller...");
            string controllerPath = Path.Combine(_projectDir, "Controllers", "OwnerController.cs");
            
            Assert.IsTrue(File.Exists(controllerPath), "OwnerController.cs missing!");
            var content = File.ReadAllText(controllerPath);

            // Verify the specific safety check logic exists
            bool hasSafetyCheck = content.Contains("AnyAsync(p => p.CategoryId == id)");
            Assert.IsTrue(hasSafetyCheck, "Safety check logic (checking for products in category) is missing in OwnerController!");
            
            Console.WriteLine("✅ Verified: Safety logic for category deletion is implemented.");
        }
    }
}
