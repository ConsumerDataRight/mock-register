# Security Policy
If you have discovered a potential security vulnerability within the [Consumer Data Right GitHub Organisation](https://github.com/ConsumerDataRight) or [Consumer Data Right Sandbox](https://cdrsandbox.gov.au/) 
operated by the ACCC, we encourage you to disclose it to us as quickly as possible and in a responsible manner in accordance with our [Responsible disclosure of security vulnerabilities policy](https://www.cdr.gov.au/resources/responsible-disclosure-security-vulnerabilities-policy).

Visit our [Responsible disclosure of security vulnerabilities policy](https://www.cdr.gov.au/resources/responsible-disclosure-security-vulnerabilities-policy) for:
 - A full view of our Responsible disclosure of security vulnerabilities policy
 - Your responsibilities if you find a vulnerability 
 - Steps required for reporting a vulnerability

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 2.1.x   | :white_check_mark: |
| 1.x.x   | :x: |

## Reporting a Vulnerability
Visit our [Responsible disclosure of security vulnerabilities policy](https://www.cdr.gov.au/resources/responsible-disclosure-security-vulnerabilities-policy) for steps required for reporting a vulnerability.


## What controls are in place
### SonarCloud
Code repositories in [Consumer Data Right GitHub Organisation](https://github.com/ConsumerDataRight) utilise [SonarCloud](https://docs.sonarcloud.io/). Whenever a code change is made to this repository, GitHub actions are used to scan the code using SonarCloud. 
The SonarCloud results are then assessed. High impact issues, that are not false positives, will be remediated.
 - [mock-register results](https://sonarcloud.io/project/overview?id=ConsumerDataRight_mock-register)
 - [mock-data-holder results](https://sonarcloud.io/project/overview?id=ConsumerDataRight_mock-data-holder)
 - [mock-data-holder-energy results](https://sonarcloud.io/project/overview?id=ConsumerDataRight_mock-data-holder-energy)
 - [mock-data-recipient results](https://sonarcloud.io/project/overview?id=ConsumerDataRight_mock-data-recipient)
 - [authorisation-server results](https://sonarcloud.io/project/overview?id=ConsumerDataRight_authorisation-server)
 - [mock-solution-test-automation results](https://sonarcloud.io/project/overview?id=ConsumerDataRight_mock-solution-test-automation)

### GitHub Security Features
Code repositories in [Consumer Data Right GitHub Organisation](https://github.com/ConsumerDataRight) utilise [GitHub security features](https://docs.github.com/en/code-security/getting-started/github-security-features).

### Keeping up to date
Code repositories in [Consumer Data Right GitHub Organisation](https://github.com/ConsumerDataRight) are routinely updated with new features and dependency updates.