using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DouYu
{
    public static class DouYuApiHelper
    {
        private static HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// 获取所有游戏分类
        /// </summary>
        /// <returns></returns>
        public static async Task<ResponseListData<GameCateItem>> GetAllGameCate()
        {
            var res = await _httpClient.GetStringAsync(DouYuApiUrls.GetRoomCateUrl);
            var baseres = JsonConvert.DeserializeObject<BaseResponse>(res);
            if (baseres.Error != 0)
            {
                var errres = JsonConvert.DeserializeObject<ErrorResponse>(res);
                return new ResponseListData<GameCateItem>
                {
                    Error = baseres.Error,
                    Data = null,
                    ErrorMsg = errres.Data
                };
            }
            else
            {
                return JsonConvert.DeserializeObject<ResponseListData<GameCateItem>>(res);
            }
        }

        /// <summary>
        /// 根据分类 ID 获取房间列表 ,cateID等于0表示搜索全部
        /// </summary>
        /// <returns></returns>
        public static async Task<ResponseListData<RoomListInfoItem>> GetRoomListByCateId(int cateId,int offset=0,int limit=30)
        {
            var res=await _httpClient.GetStringAsync(string.Format(DouYuApiUrls.GetRoomListUrl,cateId==0?"":cateId.ToString()).Trim('/'));
            var baseRes = JsonConvert.DeserializeObject<BaseResponse>(res);
            if (baseRes.Error!=0)
            {
                var errRes = JsonConvert.DeserializeObject<ErrorResponse>(res);
                return new ResponseListData<RoomListInfoItem>
                {
                    Error = errRes.Error,
                    Data = null,
                    ErrorMsg = errRes.Data
                };
            }
            else
            {
                return JsonConvert.DeserializeObject<ResponseListData<RoomListInfoItem>>(res);
            }
        }

        /// <summary>
        /// 获取房间详情
        /// </summary>
        /// <returns></returns>
        public static async Task<ResponseData<RoomDetail>> GetRoomDetail(int roomId)
        {
            var res = await _httpClient.GetStringAsync(string.Format(DouYuApiUrls.GetRoomDetail, roomId));
            var baseRes = JsonConvert.DeserializeObject<BaseResponse>(res);
            if (baseRes.Error != 0)
            {
                var errRes = JsonConvert.DeserializeObject<ErrorResponse>(res);
                return new ResponseData<RoomDetail>
                {
                    Error = errRes.Error,
                    Data = null,
                    ErrorMsg = errRes.Data
                };
            }
            else
            {
                return JsonConvert.DeserializeObject<ResponseData<RoomDetail>>(res);
            }
        }
    }

    public class ErrorMsg
    {
        public static Dictionary<int, string> _data = new Dictionary<int, string>
        {
            { 501,"不存在此分类"},
            {999,"接口维护中" },
            {101, "房间未找到（不存在此房间）" },
            {102, "房间未激活"                },
            {103, "房间获取错误" }
        }; 
    }
}

