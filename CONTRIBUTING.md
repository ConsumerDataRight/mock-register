# Contributing
We encourage contributions from the CDR community.  If you would like to get involved, please follow the guidelines below.

## Questions
Our **issues** tracker is for issues concerning the Mock Register.  It is not for general questions about the Consumer Data Right or about the Register.  General questions can be raised through the usual support channels, such as [CDR Support Portal](https://cdr-support.zendesk.com/hc/en-us).

## Pull Request Process
If you see an opportunity for improvement or identify an issue that can be fixed and you can make the change yourself, please go ahead. We follow a typical `git` workflow process for changes to the Mock Register source code.

1. Fork the Mock Register repository.  See [Fork a repo](https://docs.github.com/en/github/getting-started-with-github/fork-a-repo) on GitHub.
2. Clone the forked repository.
3. Create a new feature or hotfix branch.
4. Make your changes and push your code.
5. Open a Pull Request against the `mock-register` repository.
6. A member of the **Mock Register** team will review your change and approve or comment on it in due course.
7. Approved changes are merged to the `develop` branch.
8. A new container will be built and pushed to [Docker Hub](https://hub.docker.com/r/consumerdataright/mock-register).
9. At certain points the `develop` branch will be merged to `main` and a new container will be pushed to Docker Hub.

### Notes:
- We use a typical git workflow. Feature branches should start `feature/` and `hotfix/`.  
- Please make sure that you **ALWAYS** create pull requests for the develop branch.

## Representations
This software and associated documentation ("Work") is presented by the ACCC for the purpose of fostering innovation, free of charge, for the benefit of the public. The ACCC monitors the quality of the Work available, and responds to and updates the Work through methods such as Pull Requests.

The ACCC does not guarantee, and accepts no legal liability whatsoever arising from or connected to, the use of or reliance on any views or recommendations expressed by or on behalf of the ACCC through any responses or updates to the Work, including any representations made during a Pull Request process. Any such statements do not necessarily reflect the views of the ACCC or indicate its commitment to a particular course of action.
