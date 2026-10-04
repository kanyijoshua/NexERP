import { mapEnumToOptions } from '@abp/ng.core';

export enum RoomType {
  LectureHall = 0,
  Laboratory = 1,
}

export const roomTypeOptions = mapEnumToOptions(RoomType);
