# RxSpec
Demo app demonstrating integration of SpecFlow and Rx

One of my current projects uses SpecFlow extensively to perform BDD-style integration tests against a BizTalk solution. A challenge has emerged with this approach, in that while the expectations of the test occur asynchronously and therefore in no particular order, SpecFlow is sequential which imposes a degree of brittleness on the tests.

Therefore I have written this project as a proof-of-concept for using Rx to observe events asynchronously, where the subscription to the event stream is defined by the SpecFlow test steps.
