using Tsinswreng.CsTreeTest;
using TswgCsTpl.Test.Domains.Calculator;
namespace TswgCsTpl.Test;

public class TswgCsTplTestMgr:DiEtTestMgr{
	public static TswgCsTplTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Node){
		Node = this.TestNode;
		this.RegisterTester<TestCalculator>();
		return Node;
	}
}
