#### 0.3.1 - November 28 2014
* First version, just repacked from mmf.codeplex.com for the simplest use case.

#### 0.4.0 - November 29 2014
* Complete refactoring. Remove multiple levels of indirection and manual thread management.
* Introduced three persistence modes - permanent on disk, temporary on disk or in memory (the last is not tested).
* Use named mutexes to allow interprocess communication (multithreaded access in one process is slower.)
* Use unsafe memory read/write for faster performance.
* Change license to Apache 2.0.