# RcxAgent
A proof-of-concept agent for the Remote Commend Execution service.

This is a .NET application that hosts a WCF HTTP RESTful web service. This service enables read-only filesystem access and execution of arbitrary commands. Given that there is no encryption and no authentication, you definitely do NOT want to run this... pretty much anywhere, but certainly do NOT run it outside a sandboxed environment or on machines you even remotely care about.
