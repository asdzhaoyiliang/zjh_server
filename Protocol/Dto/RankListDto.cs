using System;
using System.Collections.Generic;

namespace Protocol.Dto
{
    [Serializable]
    public class RankListDto
    {
        public List<RankItemDto> rankList;

        public RankListDto()
        {
            rankList = new List<RankItemDto>();
        }

        public void Clean()
        {
            rankList.Clear();
        }

        public void Add(RankItemDto dto)
        {
            rankList.Add(dto);
        }
    }
}