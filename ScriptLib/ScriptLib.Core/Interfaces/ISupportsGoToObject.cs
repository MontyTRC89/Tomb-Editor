using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLib.Core.Interfaces;

public interface ISupportsGoToObject
{
	void GoToObject(string objectNamePattern, bool isCaseSensitive, bool isExactMatch, bool isRegex);
}
