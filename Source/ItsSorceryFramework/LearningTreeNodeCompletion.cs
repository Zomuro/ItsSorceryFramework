using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItsSorceryFramework
{
	public enum LearningTreeNodeCompletion : byte
	{
		Incomplete, // node is not finished but can be done
		Complete, // node is done
		Incompletable // node cannot be done due to it being an exclusive node with another node
	}
}
