﻿using ReZero.SuperAPI; 
namespace SuperAPITest.Controllers
{ 
    [Api(200100)]
    public class MyApiController
    { 
        [ApiMethod("我是A方法")]
        public int A(int i) 
        {
            return 1;
        }
    }
}