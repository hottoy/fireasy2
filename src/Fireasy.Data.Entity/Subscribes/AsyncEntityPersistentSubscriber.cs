﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fireasy.Data.Entity.Subscribes
{
    /// <summary>
    /// 异步的实体持久化的事件订阅器抽象类。
    /// </summary>
    public abstract class AsyncEntityPersistentSubscriber
    {
        /// <summary>
        /// 接收主题信息并进行处理。
        /// </summary>
        /// <param name="subject"></param>
        public async Task AcceptAsync(EntityPersistentSubject subject)
        {
            switch (subject.EventType)
            {
                case EntityPersistentEventType.BeforeCreate:
                    {
                        if (subject.Argument is EntityEventArgs arg)
                        {
                            arg.Cancel = !await OnBeforeCreateAsync(arg.Entity);
                        }
                    }
                    break;
                case EntityPersistentEventType.AfterCreate:
                    {
                        if (subject.Argument is EntityEventArgs arg)
                        {
                            await OnAfterCreateAsync(arg.Entity);
                        }
                    }

                    await OnCreateAsync(subject.EntityType);
                    break;
                case EntityPersistentEventType.BeforeUpdate:
                    {
                        if (subject.Argument is EntityEventArgs arg)
                        {
                            arg.Cancel = !await OnBeforeUpdateAsync(arg.Entity);
                        }
                    }
                    break;
                case EntityPersistentEventType.AfterUpdate:
                    {
                        if (subject.Argument is EntityEventArgs arg)
                        {
                            await OnAfterUpdateAsync(arg.Entity);
                        }
                    }

                    await OnUpdateAsync(subject.EntityType);
                    break;
                case EntityPersistentEventType.BeforeRemove:
                    {
                        if (subject.Argument is EntityEventArgs arg)
                        {
                            arg.Cancel = !await OnBeforeRemoveAsync(arg.Entity);
                        }
                    }
                    break;
                case EntityPersistentEventType.AfterRemove:
                    {
                        if (subject.Argument is EntityEventArgs arg)
                        {
                            await OnAfterRemoveAsync(arg.Entity);
                        }
                    }

                    await OnRemoveAsync(subject.EntityType);
                    break;
                case EntityPersistentEventType.BeforeBatch:
                    {
                        var arg = subject.Argument as EntitiesArgs;
                        arg.Cancel = !await OnBeforeBatchAsync(arg.Entities, arg.OperType);
                    }
                    break;
                case EntityPersistentEventType.AfterBatch:
                    {
                        var arg = subject.Argument as EntitiesArgs;
                        await OnAfterBatchAsync(arg.Entities, arg.OperType);

                        switch (arg.OperType)
                        {
                            case EntityPersistentOperater.Create:
                                await OnCreateAsync(subject.EntityType);
                                break;
                            case EntityPersistentOperater.Update:
                                await OnUpdateAsync(subject.EntityType);
                                break;
                            case EntityPersistentOperater.Remove:
                                await OnRemoveAsync(subject.EntityType);
                                break;
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// 用于实体创建之前的通知。
        /// </summary>
        /// <param name="entity">创建的实体对象。</param>
        /// <returns>取消则返回 false。</returns>
        protected virtual async Task<bool> OnBeforeCreateAsync(IEntity entity)
        {
            return true;
        }

        /// <summary>
        /// 用于实体创建之后的通知。
        /// </summary>
        /// <param name="entity">创建的实体对象。</param>
        protected virtual async Task OnAfterCreateAsync(IEntity entity)
        {
        }

        /// <summary>
        /// 用于实体创建之前的通知。
        /// </summary>
        /// <param name="entityType"></param>
        protected virtual async Task OnCreateAsync(Type entityType)
        {
        }

        /// <summary>
        /// 用于实体更新之前的通知。
        /// </summary>
        /// <param name="entity">更新的实体对象。</param>
        /// <returns>取消则返回 false。</returns>
        protected virtual async Task<bool> OnBeforeUpdateAsync(IEntity entity)
        {
            return true;
        }

        /// <summary>
        /// 用于实体更新之后的通知。
        /// </summary>
        /// <param name="entity">更新的实体对象。</param>
        protected virtual async Task OnAfterUpdateAsync(IEntity entity)
        {
        }

        /// <summary>
        /// 用于实体更新之后的通知。
        /// </summary>
        /// <param name="entityType"></param>
        protected virtual async Task OnUpdateAsync(Type entityType)
        {
        }

        /// <summary>
        /// 用于实体移除之前的通知。
        /// </summary>
        /// <param name="entity">移除的实体对象。</param>
        /// <returns>取消则返回 false。</returns>
        protected virtual async Task<bool> OnBeforeRemoveAsync(IEntity entity)
        {
            return true;
        }

        /// <summary>
        /// 用于实体移除之后的通知。
        /// </summary>
        /// <param name="entity">移除的实体对象。</param>
        protected virtual async Task OnAfterRemoveAsync(IEntity entity)
        {
        }

        /// <summary>
        /// 用于实体移除之后的通知。
        /// </summary>
        /// <param name="entityType"></param>
        protected virtual async Task OnRemoveAsync(Type entityType)
        {
        }

        /// <summary>
        /// 用于实体批量处理之前的通知。
        /// </summary>
        /// <param name="entities">批量处理的实体对象。</param>
        /// <param name="operater"></param>
        /// <returns>取消则返回 false。</returns>
        protected virtual async Task<bool> OnBeforeBatchAsync(IEnumerable<IEntity> entities, EntityPersistentOperater operater)
        {
            return true;
        }

        /// <summary>
        /// 用于实体批量处理之后的通知。
        /// </summary>
        /// <param name="entities">批量处理的实体对象。</param>
        /// <param name="operater"></param>
        protected virtual async Task OnAfterBatchAsync(IEnumerable<IEntity> entities, EntityPersistentOperater operater)
        {
        }

    }
}
