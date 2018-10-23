Some libraries are included in this repository using a so called git subtree.
A git subtree is essentially just a copy another repository that is saved in this Tomb Editor repository.
General information about git subtrees: https://www.atlassian.com/blog/git/alternatives-to-git-submodule-git-subtree

## Subtree https://github.com/ActuallyaDeviloper/DarkUI

### Pull
        git subtree pull --prefix DarkUI https://github.com/ActuallyaDeviloper/DarkUI.git master --squash

### Push
        git subtree push --prefix DarkUI https://github.com/ActuallyaDeviloper/DarkUI.git master

### Push with remote
        git remote add UpstreamDarkUI https://github.com/ActuallyaDeviloper/DarkUI.git
        git fetch UpstreamDarkUI
        git subtree push --prefix DarkUI UpstreamDarkUI master